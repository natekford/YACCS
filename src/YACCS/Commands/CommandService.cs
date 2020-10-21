using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public class CommandService : ICommandService
	{
		public ITrie<IImmutableCommand> Commands { get; protected set; }
		IReadOnlyCollection<IImmutableCommand> ICommandService.Commands => Commands;
		protected IAsyncEvent<CommandExecutedEventArgs> CommandExecutedEvent { get; set; }
		protected ICommandServiceConfig Config { get; set; }
		protected ITypeRegistry<ITypeReader> Readers { get; set; }

		public event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted
		{
			add => CommandExecutedEvent.Add(value);
			remove => CommandExecutedEvent.Remove(value);
		}

		public event AsyncEventHandler<ExceptionEventArgs<CommandExecutedEventArgs>> CommandExecutedException
		{
			add => CommandExecutedEvent.Exception.Add(value);
			remove => CommandExecutedEvent.Exception.Remove(value);
		}

		public CommandService(ICommandServiceConfig config, ITypeRegistry<ITypeReader> readers)
		{
			Commands = new CommandTrie(config.CommandNameComparer, readers);
			CommandExecutedEvent = new AsyncEvent<CommandExecutedEventArgs>();
			Config = config;
			Readers = readers;
		}

		public async Task<IResult> ExecuteAsync(IContext context, string input)
		{
			if (!TryGetArgs(input, out var args))
			{
				return QuoteMismatchResult.Instance.Sync;
			}
			if (args.Length == 0)
			{
				return CommandNotFoundResult.Instance.Sync;
			}

			var (result, best) = await GetBestMatchAsync(context, args).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}

			_ = ExecuteAsync(context, best!.Command!, best!.Args!);
			return SuccessResult.Instance.Sync;
		}

		public virtual IReadOnlyList<IImmutableCommand> Find(string input)
		{
			if (!TryGetArgs(input, out var args))
			{
				return Array.Empty<IImmutableCommand>();
			}

			var node = Commands.Root;
			for (var i = 0; i < args.Length; ++i)
			{
				if (!node.TryGetEdge(args.Span[i], out node))
				{
					break;
				}
				if (i == args.Length - 1)
				{
					return node.AllValues.ToImmutableArray();
				}
			}

			return Array.Empty<IImmutableCommand>();
		}

		public virtual async Task<(IResult, CommandScore?)> GetBestMatchAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var best = default(CommandScore);
			var cache = new PreconditionCache(context);

			var node = Commands.Root;
			for (var i = 0; i < input.Length; ++i)
			{
				if (!node.TryGetEdge(input.Span[i], out node))
				{
					break;
				}
				foreach (var command in node.DirectValues)
				{
					// Add 1 to i to account for how we're in a node
					var score = await GetCommandScoreAsync(cache, context, command, input, i + 1).ConfigureAwait(false);
					if (Config.MultiMatchHandling == MultiMatchHandling.Error
						&& best?.InnerResult.IsSuccess == true
						&& score.InnerResult.IsSuccess)
					{
						return (MultiMatchHandlingErrorResult.Instance.Sync, null);
					}
					best = CommandScore.Max(best, score);
				}
			}

			var result = best?.InnerResult ?? CommandNotFoundResult.Instance.Sync;
			return (result, best);
		}

		public ValueTask<CommandScore> GetCommandScoreAsync(
			PreconditionCache cache,
			IContext context,
			IImmutableCommand command,
			ReadOnlyMemory<string> input,
			int startIndex)
		{
			// Trivial cases, invalid context or arg length
			if (!command.IsValidContext(context.GetType()))
			{
				var score = CommandScore.FromInvalidContext(command, context, startIndex);
				return new ValueTask<CommandScore>(score);
			}
			else if (input.Length - startIndex < command.MinLength)
			{
				var score = CommandScore.FromNotEnoughArgs(command, context, startIndex);
				return new ValueTask<CommandScore>(score);
			}
			else if (input.Length - startIndex > command.MaxLength)
			{
				var score = CommandScore.FromTooManyArgs(command, context, startIndex);
				return new ValueTask<CommandScore>(score);
			}

			return new ValueTask<CommandScore>(ProcessAllPreconditionsAsync(
				cache,
				command,
				context,
				input,
				startIndex
			));
		}

		public async Task<CommandScore> ProcessAllPreconditionsAsync(
			PreconditionCache cache,
			IImmutableCommand command,
			IContext context,
			ReadOnlyMemory<string> input,
			int startIndex)
		{
			// Any precondition fails, command is not valid
			var pResult = await ProcessPreconditionsAsync(
				cache,
				command
			).ConfigureAwait(false);
			if (!pResult.IsSuccess)
			{
				return CommandScore.FromFailedPrecondition(command, context, pResult, 0);
			}

			var args = new object?[command.Parameters.Count];
			var currentIndex = startIndex;
			for (var i = 0; i < command.Parameters.Count; ++i)
			{
				var parameter = command.Parameters[i];

				var value = parameter.DefaultValue;
				// We still have more args to parse so let's look through those
				if (currentIndex < input.Length)
				{
					var trResult = await ProcessTypeReadersAsync(
						cache,
						parameter,
						input,
						currentIndex
					).ConfigureAwait(false);
					if (!trResult.InnerResult.IsSuccess)
					{
						return CommandScore.FromFailedTypeReader(command, context, trResult.InnerResult, i);
					}

					value = trResult.Value;
					currentIndex += parameter.Length ?? int.MaxValue;
				}
				// We don't have any more args to parse.
				// If the parameter isn't optional it's a failure
				else if (!parameter.HasDefaultValue)
				{
					return CommandScore.FromFailedOptionalArgs(command, context, i);
				}

				var ppResult = await ProcessParameterPreconditionsAsync(
					cache,
					command,
					parameter,
					value
				).ConfigureAwait(false);
				if (!ppResult.IsSuccess)
				{
					return CommandScore.FromFailedParameterPrecondition(command, context, ppResult, i);
				}

				args[i] = value;
			}
			return CommandScore.FromCanExecute(command, context, args);
		}

		public ValueTask<IResult> ProcessParameterPreconditionsAsync(
			PreconditionCache cache,
			IImmutableCommand command,
			IImmutableParameter parameter,
			object? value)
		{
			if (parameter.Preconditions.Count == 0)
			{
				return new ValueTask<IResult>(SuccessResult.Instance.Sync);
			}

			static async Task<IResult> ProcessParameterPreconditionsAsync(
				PreconditionCache cache,
				IImmutableCommand command,
				IImmutableParameter parameter,
				object? value)
			{
				var info = new ParameterInfo(command, parameter);
				foreach (var precondition in parameter.Preconditions)
				{
					var result = await cache.GetResultAsync(info, precondition, value).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}
				return SuccessResult.Instance.Sync;
			}

			return new ValueTask<IResult>(ProcessParameterPreconditionsAsync(
				cache,
				command,
				parameter,
				value
			));
		}

		public ValueTask<IResult> ProcessPreconditionsAsync(
			PreconditionCache cache,
			IImmutableCommand command)
		{
			if (command.Preconditions.Count == 0)
			{
				return new ValueTask<IResult>(SuccessResult.Instance.Sync);
			}

			static async Task<IResult> ProcessPreconditionsAsync(
				PreconditionCache cache,
				IImmutableCommand command)
			{
				foreach (var precondition in command.Preconditions)
				{
					var result = await cache.GetResultAsync(command, precondition).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}
				return SuccessResult.Instance.Sync;
			}

			return new ValueTask<IResult>(ProcessPreconditionsAsync(
				cache,
				command
			));
		}

		public ValueTask<ITypeReaderResult> ProcessTypeReadersAsync(
			PreconditionCache cache,
			IImmutableParameter parameter,
			ReadOnlyMemory<string> input,
			int startIndex)
		{
			var reader = Readers.Get(parameter);
			var pLength = parameter.Length ?? int.MaxValue;
			// Iterate at least once even for arguments with zero length, i.e. IContext
			var length = Math.Max(Math.Min(input.Length - startIndex, pLength), 1);
			var sliced = input.Slice(startIndex, length);
			return cache.GetResultAsync(reader, sliced);
		}

		protected virtual Task CommandFinishedAsync(IContext context, IImmutableCommand command)
		{
			if (context is IDisposable disposable)
			{
				disposable.Dispose();
			}
			return Task.CompletedTask;
		}

		protected virtual async Task ExecuteAsync(IContext context, IImmutableCommand command, object?[] args)
		{
			var exceptions = new List<Exception>();
			var exception = default(Exception?);

			// Handling preconditions which may need to modify state when the command to
			// execute has been found, e.g. cooldown or prevent duplicate long running commands
			foreach (var precondition in command.Preconditions)
			{
				try
				{
					await precondition.BeforeExecutionAsync(command, context).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}

			try
			{
				var result = await command.ExecuteAsync(context, args).ConfigureAwait(false);
				var e = new CommandExecutedEventArgs(command, context, result.InnerResult);
				await CommandExecutedEvent.InvokeAsync(e).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				exceptions.Add(e);
				exception = e;
			}
			finally
			{
				// Handling preconditions which may need to modify state after a command has
				// finished executing, e.g. on exception remove user from cooldown or release
				// long running command lock
				foreach (var precondition in command.Preconditions)
				{
					try
					{
						await precondition.AfterExecutionAsync(command, context, exception).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				}
			}

			if (exceptions.Count > 0)
			{
				var result = ExceptionDuringCommandResult.Instance.Sync;
				var e = new CommandExecutedEventArgs(command, context, result)
					.WithExceptions(exceptions);
				await CommandExecutedEvent.Exception.InvokeAsync(e).ConfigureAwait(false);
			}

			await CommandFinishedAsync(context, command).ConfigureAwait(false);
		}

		protected virtual bool TryGetArgs(
			string input,
			[NotNullWhen(true)] out ReadOnlyMemory<string> args)
		{
			if (ParseArgs.TryParse(
				input,
				Config.Separator,
				Config.StartQuotes,
				Config.EndQuotes,
				out var parsed))
			{
				args = parsed;
				return true;
			}
			args = null;
			return false;
		}
	}
}
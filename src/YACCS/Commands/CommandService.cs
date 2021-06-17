using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public class CommandService : ICommandService
	{
		public ITrie<string, IImmutableCommand> Commands { get; protected set; }
		IReadOnlyCollection<IImmutableCommand> ICommandService.Commands => Commands;
		protected IAsyncEvent<CommandExecutedEventArgs> CommandExecutedEvent { get; set; }
		protected ICommandServiceConfig Config { get; set; }
		protected IReadOnlyDictionary<Type, ITypeReader> Readers { get; set; }
		protected IArgumentHandler Splitter { get; set; }

		public event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted
		{
			add => CommandExecutedEvent.Add(value);
			remove => CommandExecutedEvent.Remove(value);
		}

		public CommandService(
			ICommandServiceConfig config,
			IArgumentHandler splitter,
			IReadOnlyDictionary<Type, ITypeReader> readers)
		{
			Config = config;
			Splitter = splitter;
			Readers = readers;

			CommandExecutedEvent = new AsyncEvent<CommandExecutedEventArgs>();
			Commands = new CommandTrie(readers, config);
		}

		public virtual Task<ICommandResult> ExecuteAsync(IContext context, string input)
		{
			if (!Splitter.TryGetArgs(input, out var args))
			{
				return CommandScore.QuoteMismatchTask;
			}
			if (args.Length == 0)
			{
				return CommandScore.CommandNotFoundTask;
			}

			return PrivateExecuteAsync(context, args);
		}

		public virtual IReadOnlyList<IImmutableCommand> Find(string input)
		{
			if (!Splitter.TryGetArgs(input, out var args))
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
					// Generated items have a source and that source gives them the same
					// names/properties, so they should be ignored since they are copies
					return node.GetAllItems(x => x.Source is null);
				}
			}

			return Array.Empty<IImmutableCommand>();
		}

		public virtual async Task<CommandScore> GetBestMatchAsync(
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
				foreach (var command in node.Items)
				{
					// Add 1 to i to account for how we're in a node
					var score = await GetCommandScoreAsync(cache, context, command, input, i + 1).ConfigureAwait(false);
					if (Config.MultiMatchHandling == MultiMatchHandling.Error
						&& best?.InnerResult.IsSuccess == true
						&& score.InnerResult.IsSuccess)
					{
						return CommandScore.MultiMatch;
					}
					best = CommandScore.Max(best, score);
				}
			}

			return best ?? CommandScore.CommandNotFound;
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
				context,
				command,
				input,
				startIndex
			));
		}

		public async Task<CommandScore> ProcessAllPreconditionsAsync(
			PreconditionCache cache,
			IContext context,
			IImmutableCommand command,
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
				return CommandScore.FromFailedPrecondition(command, context, pResult, startIndex);
			}

			var args = new object?[command.Parameters.Count];
			var currentIndex = startIndex;
			for (var i = 0; i < command.Parameters.Count; ++i)
			{
				var parameter = command.Parameters[i];

				var value = parameter.DefaultValue;
				// We still have more args to parse so let's look through those
				if (currentIndex < input.Length || parameter.Length is null)
				{
					var trResult = await ProcessTypeReadersAsync(
						cache,
						parameter,
						input,
						currentIndex
					).ConfigureAwait(false);
					if (!trResult.InnerResult.IsSuccess)
					{
						return CommandScore.FromFailedTypeReader(command, parameter, context, trResult.InnerResult, currentIndex - 1);
					}

					value = trResult.Value;

					// Length not null, we can just add it
					if (parameter.Length is not null)
					{
						currentIndex += parameter.Length.Value;
					}
					// Last parameter, indicate we're absolutely done via int.MaxValue
					else if (i == command.Parameters.Count - 1)
					{
						currentIndex = int.MaxValue;
					}
					// Middle parameter, just go onto next parameter without moving the index
					// This won't really ever happen in most well designed commands, but with
					// delegate commands this is the only way to get context passed to it
				}
				// We don't have any more args to parse.
				// If the parameter isn't optional it's a failure
				else if (!parameter.HasDefaultValue)
				{
					return CommandScore.FromFailedOptionalArgs(command, parameter, context, currentIndex - 1);
				}

				var ppResult = await ProcessParameterPreconditionsAsync(
					cache,
					command,
					parameter,
					value
				).ConfigureAwait(false);
				if (!ppResult.IsSuccess)
				{
					return CommandScore.FromFailedParameterPrecondition(command, parameter, context, ppResult, currentIndex - 1);
				}

				args[i] = value;
			}
			return CommandScore.FromCanExecute(command, context, args, startIndex + 1);
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
				foreach (var precondition in parameter.Preconditions)
				{
					var result = await cache.GetResultAsync(command, parameter, precondition, value).ConfigureAwait(false);
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
				// Preconditions are grouped but cannot be subgrouped
				// So treat logic as group is surrounded by parantheses but inside isn't
				// Each group must succeed for a command to be valid
				foreach (var group in command.Preconditions)
				{
					IResult groupResult = SuccessResult.Instance.Sync;
					foreach (var precondition in group.Value)
					{
						// An AND has already failed, no need to check other ANDs
						if (precondition.Op == BoolOp.And && !groupResult.IsSuccess)
						{
							continue;
						}

						var result = await cache.GetResultAsync(command, precondition).ConfigureAwait(false);
						// OR: Any success = instant success, go to next group
						if (precondition.Op == BoolOp.Or && result.IsSuccess)
						{
							groupResult = SuccessResult.Instance.Sync;
							break;
						}
						// AND: Any failure = skip other ANDs, only check further ORs
						else if (precondition.Op == BoolOp.And && !result.IsSuccess)
						{
							groupResult = result;
						}
					}
					if (!groupResult.IsSuccess)
					{
						return groupResult;
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
			var reader = Readers.GetTypeReader(parameter);
			var pLength = parameter.Length ?? int.MaxValue;
			// Allow a length of zero for arguments with zero length, i.e. IContext
			var length = Math.Max(Math.Min(input.Length - startIndex, pLength), 0);
			var sliced = input.Slice(startIndex, length);
			return cache.GetResultAsync(reader, sliced);
		}

		protected virtual Task DisposeCommandAsync(CommandExecutedEventArgs e)
		{
			if (e.Context is IDisposable disposable)
			{
				disposable.Dispose();
			}
			return Task.CompletedTask;
		}

		protected virtual async Task<CommandExecutedEventArgs> HandleCommandAsync(
			IContext context,
			IImmutableCommand command,
			object?[] args)
		{
			var beforeExceptions = default(List<Exception>?);
			var afterExceptions = default(List<Exception>?);

			// Handling preconditions which may need to modify state when the command to
			// execute has been found, e.g. cooldown or prevent duplicate long running commands
			foreach (var group in command.Preconditions)
			{
				foreach (var precondition in group.Value)
				{
					try
					{
						await precondition.BeforeExecutionAsync(command, context).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						beforeExceptions ??= new();
						beforeExceptions.Add(ex);
					}
				}
			}

			IResult? result;
			var duringException = default(Exception?);
			try
			{
				result = await command.ExecuteAsync(context, args).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				result = ExceptionDuringCommandResult.Instance.Sync;
				duringException = ex;
			}

			// Handling preconditions which may need to modify state after a command has
			// finished executing, e.g. on exception remove user from cooldown or release
			// long running command lock
			foreach (var group in command.Preconditions)
			{
				foreach (var precondition in group.Value)
				{
					try
					{
						await precondition.AfterExecutionAsync(command, context, duringException).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						afterExceptions ??= new();
						afterExceptions.Add(ex);
					}
				}
			}

			return new CommandExecutedEventArgs(
				command,
				context,
				beforeExceptions,
				afterExceptions,
				duringException,
				result
			);
		}

		private async Task<ICommandResult> PrivateExecuteAsync(
			IContext context,
			ReadOnlyMemory<string> args)
		{
			var best = await GetBestMatchAsync(context, args).ConfigureAwait(false);
			// If a command is found and args are parsed, execute command in background
			if (best.InnerResult.IsSuccess && best.Command != null && best.Args != null)
			{
				_ = PrivateHandleCommandAsync(context, best.Command, best.Args);
			}
			return best;
		}

		private async Task PrivateHandleCommandAsync(
			IContext context,
			IImmutableCommand command,
			object?[] args)
		{
			var e = await HandleCommandAsync(context, command, args).ConfigureAwait(false);
			await CommandExecutedEvent.InvokeAsync(e).ConfigureAwait(false);
			await DisposeCommandAsync(e).ConfigureAwait(false);
		}
	}
}
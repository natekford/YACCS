using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public abstract class CommandServiceBase : ICommandService
	{
		public ITrie<string, IImmutableCommand> Commands { get; set; }
		IReadOnlyCollection<IImmutableCommand> ICommandService.Commands => Commands;
		protected ICommandServiceConfig Config { get; set; }
		protected IArgumentHandler Handler { get; set; }
		protected IReadOnlyDictionary<Type, ITypeReader> Readers { get; set; }

		protected CommandServiceBase(
			ICommandServiceConfig config,
			IArgumentHandler handler,
			IReadOnlyDictionary<Type, ITypeReader> readers)
		{
			Config = config;
			Readers = readers;
			Handler = handler;
			Commands = new CommandTrie(readers, config);
		}

		public virtual ValueTask<ICommandResult> ExecuteAsync(IContext context, string input)
		{
			if (!Handler.TrySplit(input, out var args))
			{
				return new(CommandScore.QuoteMismatch);
			}
			if (args.Length == 0)
			{
				return new(CommandScore.CommandNotFound);
			}
			return PrivateExecuteAsync(context, args);
		}

		public virtual IReadOnlyCollection<IImmutableCommand> Find(ReadOnlyMemory<string> input)
		{
			var node = Commands.Root;
			foreach (var key in input.Span)
			{
				if (!node.TryGetEdge(key, out node))
				{
					break;
				}
			}
			if (node is null)
			{
				return Array.Empty<IImmutableCommand>();
			}

			// Generated items have a source and that source gives them the same
			// names/properties, so they should be ignored since they are copies
			return node.GetAllDistinctItems(x => x.Source is null);
		}

		protected internal virtual async ValueTask<CommandScore> GetBestMatchAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var best = default(CommandScore);

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
					var score = await GetCommandScoreAsync(context, command, input, i + 1).ConfigureAwait(false);
					if (Config.MultiMatchHandling == MultiMatchHandling.Error
						&& best?.InnerResult.IsSuccess == true
						&& score.InnerResult.IsSuccess)
					{
						return CommandScore.MultiMatch;
					}
					best = Max(best, score);
				}
			}

			return best ?? CommandScore.CommandNotFound;
		}

		protected internal ValueTask<CommandScore> GetCommandScoreAsync(
			IContext context,
			IImmutableCommand command,
			ReadOnlyMemory<string> input,
			int startIndex)
		{
			// Trivial cases, invalid context or arg length
			if (!command.IsValidContext(context.GetType()))
			{
				return new(CommandScore.FromInvalidContext(command, context, startIndex));
			}
			else if (input.Length - startIndex < command.MinLength)
			{
				return new(CommandScore.FromNotEnoughArgs(command, context, startIndex));
			}
			else if (input.Length - startIndex > command.MaxLength)
			{
				return new(CommandScore.FromTooManyArgs(command, context, startIndex));
			}

			return ProcessAllPreconditionsAsync(
				context,
				command,
				input,
				startIndex
			);
		}

		protected internal async ValueTask<CommandScore> ProcessAllPreconditionsAsync(
			IContext context,
			IImmutableCommand command,
			ReadOnlyMemory<string> input,
			int startIndex)
		{
			// Any precondition fails, command is not valid
			var pResult = await command.Preconditions
				.ProcessAsync(x => x.CheckAsync(command, context))
				.ConfigureAwait(false);
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
						context,
						parameter,
						input,
						currentIndex
					).ConfigureAwait(false);
					if (!trResult.InnerResult.IsSuccess)
					{
						return CommandScore.FromFailedTypeReader(command, parameter, context, trResult.InnerResult, currentIndex);
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
					return CommandScore.FromFailedOptionalArgs(command, parameter, context, currentIndex);
				}

				var meta = new CommandMeta(command, parameter);
				var ppResult = await parameter.Preconditions
					.ProcessAsync(x => x.CheckAsync(meta, context, value))
					.ConfigureAwait(false);
				if (!ppResult.IsSuccess)
				{
					return CommandScore.FromFailedParameterPrecondition(command, parameter, context, ppResult, currentIndex);
				}

				args[i] = value;
			}
			return CommandScore.FromCanExecute(command, context, args, startIndex + 1);
		}

		protected internal ITask<ITypeReaderResult> ProcessTypeReadersAsync(
			IContext context,
			IImmutableParameter parameter,
			ReadOnlyMemory<string> input,
			int startIndex)
		{
			var reader = Readers.GetTypeReader(parameter);
			var pLength = parameter.Length ?? int.MaxValue;
			// Allow a length of zero for arguments with zero length, i.e. IContext
			var length = Math.Max(Math.Min(input.Length - startIndex, pLength), 0);
			var sliced = input.Slice(startIndex, length);
			return reader.ReadAsync(context, sliced);
		}

		protected virtual Task DisposeCommandAsync(CommandExecutedEventArgs e)
		{
			if (e.Context is IDisposable disposable)
			{
				disposable.Dispose();
			}
			return Task.CompletedTask;
		}

		protected virtual async ValueTask<CommandExecutedEventArgs> HandleCommandAsync(
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
				result = ExceptionDuringCommandResult.Instance;
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

			return new(
				command,
				context,
				beforeExceptions,
				afterExceptions,
				duringException,
				result
			);
		}

		protected virtual CommandScore? Max(CommandScore? a, CommandScore? b)
			=> CommandScore.Max(a, b);

		protected abstract Task OnCommandExecutedAsync(CommandExecutedEventArgs e);

		private async ValueTask<ICommandResult> PrivateExecuteAsync(
			IContext context,
			ReadOnlyMemory<string> args)
		{
			var best = await GetBestMatchAsync(context, args).ConfigureAwait(false);
			// If a command is found and args are parsed, execute command in background
			if (best.InnerResult.IsSuccess && best.Command is not null && best.Args is not null)
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
			await OnCommandExecutedAsync(e).ConfigureAwait(false);
			await DisposeCommandAsync(e).ConfigureAwait(false);
		}
	}
}
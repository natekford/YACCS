using MorseCode.ITask;

using YACCS.Commands.CommandScores;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.Results;
using YACCS.Trie;
using YACCS.TypeReaders;

namespace YACCS.Commands;

/// <summary>
/// The base class for a command service.
/// </summary>
public abstract class CommandServiceBase : ICommandService
{
	/// <inheritdoc cref="ICommandService.Commands"/>
	public virtual ITrie<string, IImmutableCommand> Commands { get; }
	IReadOnlyTrie<string, IImmutableCommand> ICommandService.Commands => Commands;
	/// <summary>
	/// The configuration to use.
	/// </summary>
	protected virtual CommandServiceConfig Config { get; }
	/// <summary>
	/// The argument handler to use for splitting input.
	/// </summary>
	protected virtual IArgumentHandler Handler { get; }
	/// <summary>
	/// The type readers to use for parsing.
	/// </summary>
	protected virtual IReadOnlyDictionary<Type, ITypeReader> Readers { get; }

	/// <summary>
	/// Creates a new <see cref="CommandServiceBase"/>.
	/// </summary>
	/// <param name="config">
	/// <inheritdoc cref="Config" path="/summary"/>
	/// </param>
	/// <param name="handler">
	/// <inheritdoc cref="Handler" path="/summary"/>
	/// </param>
	/// <param name="readers">
	/// <inheritdoc cref="Readers" path="/summary"/>
	/// </param>
	protected CommandServiceBase(
		CommandServiceConfig config,
		IArgumentHandler handler,
		IReadOnlyDictionary<Type, ITypeReader> readers)
	{
		Config = config;
		Readers = readers;
		Handler = handler;
		Commands = new CommandTrie(readers, config.Separator, config.CommandNameComparer);
	}

	/// <inheritdoc cref="ICommandService.ExecuteAsync(IContext, ReadOnlySpan{char})" />
	/// <returns>A failure result or <see cref="Success.Instance"/>.</returns>
	/// <inheritdoc cref="IExecuteResult" path="/remarks"/>
	public virtual ValueTask<IExecuteResult> ExecuteAsync(
		IContext context,
		ReadOnlySpan<char> input)
	{
		if (!Handler.TrySplit(input, out var args))
		{
			return new(CommandScore.QuoteMismatch);
		}
		if (args.Length == 0)
		{
			return new(CommandScore.CommandNotFound);
		}
		return ExecuteAsync(context, args);

		async ValueTask<IExecuteResult> ExecuteAsync(IContext context, ReadOnlyMemory<string> args)
		{
			var best = await GetBestMatchAsync(context, args).ConfigureAwait(false);
			// If a command is found and args are parsed, execute command in background
			if (best.InnerResult.IsSuccess && best.Command is not null && best.Args is not null)
			{
				_ = Task.Run(async () =>
				{
					var e = await this.ExecuteAsync(best.Context, best.Command, best.Args).ConfigureAwait(false);
					await CommandExecutedAsync(e).ConfigureAwait(false);
					await CommandFinishedAsync(e).ConfigureAwait(false);
				});
			}
			return best;
		}
	}

	Task ICommandService.ExecuteAsync(IContext context, ReadOnlySpan<char> input)
		=> ExecuteAsync(context, input).AsTask();

	/// <summary>
	/// Steps through <see cref="Commands"/> and parses each command at every edge it
	/// goes to.
	/// </summary>
	/// <param name="context">The context invoking a command.</param>
	/// <param name="input">The input being parsed.</param>
	/// <returns>
	/// The highest rated command score, this still can indicate either
	/// success or failure.
	/// </returns>
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
			foreach (var command in node)
			{
				// Add 1 to i to account for how we're in a node
				var score = await GetCommandScoreAsync(context, command, input, i + 1).ConfigureAwait(false);
				if (Config.MultiMatchHandling == MultiMatchHandling.Error
					&& best?.InnerResult.IsSuccess == true
					&& score.InnerResult.IsSuccess)
				{
					return CommandScore.MultiMatch;
				}
				best = GetBest(score, best);
			}
		}

		return best ?? CommandScore.CommandNotFound;
	}

	/// <summary>
	/// Checks trivial cases like invalid context or invalid arg length, then invokes
	/// <see cref="ProcessAllPreconditionsAsync(IContext, IImmutableCommand, ReadOnlyMemory{string}, int)"/>.
	/// </summary>
	/// <inheritdoc cref="ProcessAllPreconditionsAsync(IContext, IImmutableCommand, ReadOnlyMemory{string}, int)"/>
	protected internal virtual ValueTask<CommandScore> GetCommandScoreAsync(
		IContext context,
		IImmutableCommand command,
		ReadOnlyMemory<string> input,
		int startIndex)
	{
		// Trivial cases, invalid context or arg length
		if (!command.IsValidContext(context.GetType()))
		{
			return new(context.InvalidContext(command, startIndex));
		}
		else if (input.Length - startIndex < command.MinLength)
		{
			return new(context.NotEnoughArgs(command, startIndex));
		}
		else if (input.Length - startIndex > command.MaxLength)
		{
			return new(context.TooManyArgs(command, startIndex));
		}
		return ProcessAllPreconditionsAsync(context, command, input, startIndex);
	}

	/// <summary>
	/// Processes all preconditions: command preconditions, argument parsing, and parameter
	/// preconditions. If any single item fails, the returned <see cref="CommandScore"/>
	/// will indicate failure.
	/// </summary>
	/// <param name="context">The context invoking a command.</param>
	/// <param name="command">The command having its preconditions be parsed.</param>
	/// <param name="input">The input being parsed.</param>
	/// <param name="startIndex">The index to start at for <paramref name="input"/>.</param>
	/// <returns>A command score indicating success or failure.</returns>
	protected internal virtual async ValueTask<CommandScore> ProcessAllPreconditionsAsync(
		IContext context,
		IImmutableCommand command,
		ReadOnlyMemory<string> input,
		int startIndex)
	{
		// Any precondition fails, command is not valid
		var pResult = await command.CanExecuteAsync(context).ConfigureAwait(false);
		if (!pResult.IsSuccess)
		{
			return context.FailedPrecondition(command, startIndex, pResult);
		}

		var args = default(object?[]);
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
					return context.FailedTypeReader(command, currentIndex, trResult.InnerResult, parameter);
				}

				value = trResult.Value;

				var spCount = trResult.SuccessfullyParsedCount;
				var pLength = parameter.Length;
				var indexDelta = spCount ?? pLength;
				// Use the minimum value of successfully parsed count and parameter length
				// to prevent results with some nonsensically large parsed count from
				// advancing to an index past where their input stopped
				if (indexDelta.HasValue)
				{
					if (spCount.HasValue && pLength.HasValue)
					{
						indexDelta = Math.Min(indexDelta.Value, pLength.Value);
					}
					currentIndex += indexDelta.Value;
				}

				// In case of overflow, set value to something reasonable
				if (!indexDelta.HasValue || currentIndex < 0)
				{
					currentIndex = input.Length;
				}
			}
			// We don't have any more args to parse.
			// If the parameter isn't optional it's a failure
			else if (!parameter.HasDefaultValue)
			{
				return context.MissingParameterValue(command, currentIndex, parameter);
			}

			var ppResult = await command.CanExecuteAsync(parameter, context, value).ConfigureAwait(false);
			if (!ppResult.IsSuccess)
			{
				return context.FailedParameterPrecondition(command, currentIndex, ppResult, parameter);
			}

			args ??= new object?[command.Parameters.Count];
			args[i] = value;
		}
		return context.CanExecute(command, currentIndex, args ?? []);
	}

	/// <summary>
	/// Parses a value for <paramref name="parameter"/> from <paramref name="input"/>.
	/// </summary>
	/// <param name="context">The context invoking a command.</param>
	/// <param name="parameter">The parameter being parsed for.</param>
	/// <param name="input">The input being parsed.</param>
	/// <param name="startIndex">The index to start at for <paramref name="input"/>.</param>
	/// <returns>A result indicating success or failure.</returns>
	protected internal virtual ITask<ITypeReaderResult> ProcessTypeReadersAsync(
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

	/// <summary>
	/// Called directly after the command has been executed.
	/// </summary>
	/// <param name="e">The command executed args.</param>
	/// <returns></returns>
	protected abstract Task CommandExecutedAsync(CommandExecutedEventArgs e);

	/// <summary>
	/// Called at the very end, handles disposing the context and other cleanup.
	/// </summary>
	/// <param name="e">The command executed args.</param>
	/// <returns></returns>
	protected virtual Task CommandFinishedAsync(CommandExecutedEventArgs e)
	{
		if (e.Context is IAsyncDisposable asyncDisposable)
		{
			return asyncDisposable.DisposeAsync().AsTask();
		}
		else if (e.Context is IDisposable disposable)
		{
			disposable.Dispose();
		}
		return Task.CompletedTask;
	}

	/// <summary>
	/// Executes a command, collects all the exceptions that occurs, and then creates
	/// <see cref="CommandExecutedEventArgs"/>.
	/// </summary>
	/// <param name="context">The context which is executing a command.</param>
	/// <param name="command">The command being executed.</param>
	/// <param name="args">The arguments for the command.</param>
	/// <returns>
	/// A <see cref="CommandExecutedEventArgs"/> containing the result of this method
	/// and any exceptions that occurred.
	/// </returns>
	protected virtual async ValueTask<CommandExecutedEventArgs> ExecuteAsync(
		IContext context,
		IImmutableCommand command,
		IReadOnlyList<object?> args)
	{
		var beforeExceptions = default(List<Exception>?);
		var afterExceptions = default(List<Exception>?);

		// Handling preconditions which may need to modify state when the command to
		// execute has been found, i.e. cooldown or prevent duplicate long running commands
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
					beforeExceptions ??= [];
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
			result = ExceptionDuringCommand.Instance;
			duringException = ex;
		}

		// Handling preconditions which may need to modify state after a command has
		// finished executing, i.e. on exception remove user from cooldown or release
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
					afterExceptions ??= [];
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

	/// <summary>
	/// Gets the highest rated command score.
	/// </summary>
	/// <param name="new">The newly created command score.</param>
	/// <param name="best">The current best command score.</param>
	/// <returns>The highest rated command score.</returns>
	protected virtual CommandScore GetBest(CommandScore @new, CommandScore? best)
	{
		if (best is null)
		{
			return @new;
		}

		// If a CanExecute but b cannot, a > b and vice versa
		// The instant a single command can execute, all failed commands are irrelevant
		if (@new.Stage != best.Stage)
		{
			if (@new.Stage == CommandStage.CanExecute)
			{
				return @new;
			}
			else if (best.Stage == CommandStage.CanExecute)
			{
				return best;
			}
		}

		static double GetModifier(CommandStage stage)
		{
			return stage switch
			{
				CommandStage.BadContext => 0,
				CommandStage.BadArgCount => 0.1,
				CommandStage.FailedPrecondition => 0.4,
				CommandStage.FailedTypeReader => 0.5,
				CommandStage.FailedParameterPrecondition => 0.75,
				CommandStage.CanExecute => 1,
				_ => throw new ArgumentOutOfRangeException(nameof(stage)),
			};
		}

		var scoreNew = GetModifier(@new.Stage) * (@new.Index + @new.Command?.Priority);
		var scoreOld = GetModifier(best.Stage) * (best.Index + best.Command?.Priority);
		return scoreNew > scoreOld ? @new : best;
	}
}
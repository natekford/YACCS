using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
		private readonly AsyncEvent<CommandExecutedEventArgs> _CommandExecuted
			= new AsyncEvent<CommandExecutedEventArgs>();
		private readonly CommandTrie _CommandTrie;
		private readonly ICommandServiceConfig _Config;
		private readonly ITypeReaderRegistry _Readers;

		public IReadOnlyCollection<IImmutableCommand> Commands => _CommandTrie.ToArray();

		public event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted
		{
			add => _CommandExecuted.Add(value);
			remove => _CommandExecuted.Remove(value);
		}

		public event AsyncEventHandler<ExceptionEventArgs<CommandExecutedEventArgs>> CommandExecutedException
		{
			add => _CommandExecuted.Exception.Add(value);
			remove => _CommandExecuted.Exception.Remove(value);
		}

		public CommandService(ICommandServiceConfig config, ITypeReaderRegistry readers)
		{
			_CommandTrie = new CommandTrie(config.CommandNameComparer);
			_Config = config;
			_Readers = readers;
		}

		public void Add(IImmutableCommand command)
		{
			foreach (var parameter in command.Parameters)
			{
				if (parameter.OverriddenTypeReader == null
					&& !_Readers.TryGetReader(parameter.ParameterType, out _)
					&& (parameter.EnumerableType == null || !_Readers.TryGetReader(parameter.EnumerableType, out _)))
				{
					var param = parameter.ParameterType.Name;
					var cmd = command.Names?.FirstOrDefault()?.ToString() ?? "NO NAME";
					throw new ArgumentException($"A type reader for {param} in {cmd} is missing.", nameof(command));
				}
			}
			_CommandTrie.Add(command);
		}

		public async Task<IResult> ExecuteAsync(IContext context, string input)
		{
			if (!ParseArgs.TryParse(
				input,
				_Config.StartQuotes,
				_Config.EndQuotes,
				_Config.Separator,
				out var parseArgs))
			{
				return QuoteMismatchResult.Instance.Sync;
			}

			var args = parseArgs.Arguments;
			if (args.Count == 0)
			{
				return CommandNotFoundResult.Instance.Sync;
			}

			var commands = GetPotentiallyExecutableCommands(context, args);
			if (commands.Count == 0)
			{
				return CommandNotFoundResult.Instance.Sync;
			}

			var scores = await ProcessAllPreconditionsAsync(commands, context, args).ConfigureAwait(false);
			var highestScore = scores[0];
			if (!highestScore.InnerResult.IsSuccess)
			{
				return highestScore.InnerResult;
			}
			if (scores.Count > 1
				&& scores[1].InnerResult.IsSuccess
				&& _Config.MultiMatchHandling == MultiMatchHandling.Error)
			{
				return MultiMatchHandlingErrorResult.Instance.Sync;
			}

			_ = ExecuteCommand(highestScore, context);
			return SuccessResult.Instance.Sync;
		}

		public IReadOnlyList<IImmutableCommand> Find(string input)
		{
			if (!ParseArgs.TryParse(
				input,
				_Config.StartQuotes,
				_Config.EndQuotes,
				_Config.Separator,
				out var parseArgs) || parseArgs.Arguments.Count == 0)
			{
				return Array.Empty<IImmutableCommand>();
			}

			var args = parseArgs.Arguments;
			var node = _CommandTrie.Root;
			for (var i = 0; i < args.Count; ++i)
			{
				if (!node.TryGetEdge(args[i], out node))
				{
					break;
				}
				if (i == args.Count - 1)
				{
					return node.GetCommands().ToImmutableArray();
				}
			}
			return Array.Empty<IImmutableCommand>();
		}

		public IReadOnlyList<CommandScore> GetPotentiallyExecutableCommands(
			IContext context,
			IReadOnlyList<string> input)
		{
			var count = input.Count;
			var contextType = context?.GetType();

			var node = _CommandTrie.Root;
			var matches = new List<CommandScore>();
			for (var i = 0; i < count; ++i)
			{
				if (!node.TryGetEdge(input[i], out node))
				{
					break;
				}

				foreach (var command in node.Values)
				{
					if (!command.IsValidContext(contextType))
					{
						matches.Add(CommandScore.FromInvalidContext(command, context!, i + 1));
						continue;
					}

					var (min, max) = GetMinAndMaxArgs(command);
					// Trivial cases, provided input length is not in the possible arg length
					if (count < min)
					{
						matches.Add(CommandScore.FromNotEnoughArgs(command, context!, i + 1));
					}
					else if (count > max)
					{
						matches.Add(CommandScore.FromTooManyArgs(command, context!, i + 1));
					}
					else
					{
						matches.Add(CommandScore.FromCorrectArgCount(command, context!, i + 1));
					}
				}
			}
			return SortMatches(matches);
		}

		public async Task<CommandScore> ProcessAllPreconditionsAsync(
			PreconditionCache cache,
			IImmutableCommand command,
			IContext context,
			IReadOnlyList<string> input,
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

				var arg = parameter.DefaultValue;
				// We still have more args to parse so let's look through those
				if (currentIndex < input.Count)
				{
					var trResult = await ProcessTypeReadersAsync(
						cache,
						parameter,
						input,
						startIndex
					).ConfigureAwait(false);
					if (!trResult.IsSuccess)
					{
						return CommandScore.FromFailedTypeReader(command, context, trResult, i);
					}

					arg = trResult.Value;
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
					arg
				).ConfigureAwait(false);
				if (!ppResult.IsSuccess)
				{
					return CommandScore.FromFailedParameterPrecondition(command, context, ppResult, i);
				}

				args[i] = arg;
			}
			return CommandScore.FromCanExecute(command, context, args);
		}

		public async Task<IReadOnlyList<CommandScore>> ProcessAllPreconditionsAsync(
			IReadOnlyList<CommandScore> candidates,
			IContext context,
			IReadOnlyList<string> input)
		{
			var contextType = context.GetType();
			var cache = new PreconditionCache(context);
			var matches = new List<CommandScore>();
			for (var i = 0; i < candidates.Count; ++i)
			{
				var candidate = candidates[i];

				// Only allow newly found commands with CorrectArgCount
				// Invalid args counts or any failures means don't check
				if (candidate.Stage != CommandStage.CorrectArgCount)
				{
					continue;
				}
				if (candidate.Command?.IsValidContext(contextType) != true)
				{
					continue;
				}

				matches.Add(await ProcessAllPreconditionsAsync(
					cache,
					candidate.Command!,
					context,
					input,
					candidate.Score
				).ConfigureAwait(false));
			}
			return SortMatches(matches);
		}

		public async Task<IResult> ProcessParameterPreconditionsAsync(
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

		public async Task<IResult> ProcessPreconditionsAsync(
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

		public async Task<ITypeReaderResult> ProcessTypeReadersAsync(
			PreconditionCache cache,
			IImmutableParameter parameter,
			IReadOnlyList<string> input,
			int startIndex)
		{
			var (makeArray, reader) = GetReader(parameter);
			var pLength = parameter.Length ?? int.MaxValue;
			// Iterate at least once even for arguments with zero length, i.e. IContext
			var length = Math.Min(input.Count - startIndex, Math.Max(pLength, 1));

			// If not an array, join all the values and treat them as a single string
			if (!makeArray)
			{
				var parts = input.Skip(startIndex).Take(length);
				var joined = string.Join(_Config.Separator, parts);
				return await cache.GetResultAsync(reader, joined).ConfigureAwait(false);
			}

			// If an array, test each value one by one
			var results = new ITypeReaderResult[length];
			for (var i = startIndex; i < startIndex + length; ++i)
			{
				var result = await cache.GetResultAsync(reader, input[i]).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
				results[i - startIndex] = result;
			}

			// Copy the values from the type reader result list to an array of the parameter type
			var output = Array.CreateInstance(parameter.EnumerableType, results.Length);
			for (var i = 0; i < results.Length; ++i)
			{
				output.SetValue(results[i].Value, i);
			}
			return TypeReaderResult.FromSuccess(output);
		}

		public void Remove(IImmutableCommand command)
			=> _CommandTrie.Remove(command);

		private static (int Min, int Max) GetMinAndMaxArgs(IImmutableCommand command)
		{
			var @base = command.Names[0].Parts.Count;
			var min = @base;
			var max = @base;
			foreach (var parameter in command.Parameters)
			{
				// Remainder will always be the last parameter
				if (!parameter.Length.HasValue)
				{
					max = int.MaxValue;
					break;
				}
				if (!parameter.HasDefaultValue)
				{
					min += parameter.Length.Value;
				}
				max += parameter.Length.Value;
			}
			return (min, max);
		}

		private static List<CommandScore> SortMatches(List<CommandScore> matches)
		{
			// CompareTo backwards since we want the higher values in front
			matches.Sort((x, y) => y.CompareTo(x));
			return matches;
		}

		private async Task ExecuteCommand(CommandScore score, IContext context)
		{
			var command = score.Command!;

			var exceptions = new List<Exception>();
			var exceptionResult = default(IResult);
			try
			{
				var args = score.Args!;
				var result = await command.ExecuteAsync(context, args).ConfigureAwait(false);
				var e = new CommandExecutedEventArgs(command, context, result);
				await _CommandExecuted.InvokeAsync(e).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				exceptionResult ??= ExceptionDuringCommandResult.Instance.Sync;
				exceptions.Add(ex);
			}
			finally
			{
				// Execute each postcondition, like adding in a user to a ratelimit precondition
				foreach (var precondition in command.Get<IPrecondition>())
				{
					try
					{
						await precondition.AfterExecutionAsync(command, context).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						exceptionResult ??= ExceptionAfterCommandResult.Instance.Sync;
						exceptions.Add(ex);
					}
				}
			}

			if (exceptions.Count > 0 && exceptionResult is not null)
			{
				var e = new CommandExecutedEventArgs(command, context, exceptionResult)
					.WithExceptions(exceptions);
				await _CommandExecuted.Exception.InvokeAsync(e).ConfigureAwait(false);
			}
		}

		private (bool, ITypeReader) GetReader(IImmutableParameter parameter)
		{
			// TypeReader is overridden, we /shouldn't/ need to deal with converting
			// to an enumerable for the dev
			if (parameter.OverriddenTypeReader is not null)
			{
				return (false, parameter.OverriddenTypeReader);
			}
			// Parameter type is directly in the TypeReader collection, use that
			var pType = parameter.ParameterType;
			if (_Readers.TryGetReader(pType, out var reader))
			{
				return (false, reader);
			}
			// Parameter type is not, but the parameter is an enumerable and its enumerable
			// type is in the TypeReader collection.
			// Let's read each value for the enumerable separately
			var eType = parameter.EnumerableType;
			if (eType is not null && _Readers.TryGetReader(eType, out reader))
			{
				return (true, reader);
			}
			throw new ArgumentException($"There is no converter specified for {parameter.ParameterType.Name}.");
		}
	}
}
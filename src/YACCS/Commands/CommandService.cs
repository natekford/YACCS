using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Parsing;
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

		public IReadOnlyCollection<IImmutableCommand> Commands => _CommandTrie.GetCommands();

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
				if (parameter.OverriddenTypeReader != null)
				{
					continue;
				}
				if (!_Readers.TryGetReader(parameter.ParameterType, out _))
				{
					throw new ArgumentException($"{parameter.ParameterType} is missing a type reader.", nameof(command));
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
				_Config.Separators,
				out var parseArgs))
			{
				return QuoteMismatchResult.Instance;
			}

			var args = parseArgs.Arguments;
			var commands = GetCommands(context, args);
			if (commands.Count == 0)
			{
				return CommandNotFoundResult.Instance;
			}

			var scores = await ProcessAllPreconditionsAsync(commands, context, args).ConfigureAwait(false);

			var highestScore = scores[0];
			if (!highestScore.Result.IsSuccess)
			{
				return highestScore.Result;
			}
			if (scores.Count > 1
				&& scores[1].Result.IsSuccess
				&& _Config.MultiMatchHandling == MultiMatchHandling.Error)
			{
				return MultiMatchHandlingErrorResult.Instance;
			}

			_ = ExecuteCommand(highestScore, context);
			return SuccessResult.Instance;
		}

		public async Task ExecuteCommand(CommandScore score, IContext context)
		{
			var command = score.Command!;
			var args = score.Args!;

			try
			{
				var result = await command.ExecuteAsync(context, args).ConfigureAwait(false);
				var e = new CommandExecutedEventArgs(command, context, result);
				await _CommandExecuted.InvokeAsync(e).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var result = new ExceptionResult(ex);
				var e = new CommandExecutedEventArgs(command, context, result)
					.WithExceptions(ex);
				await _CommandExecuted.Exception.InvokeAsync(e).ConfigureAwait(false);
			}
		}

		public ImmutableArray<IImmutableCommand> Find(string input)
		{
			if (!ParseArgs.TryParse(
				input,
				_Config.StartQuotes,
				_Config.EndQuotes,
				_Config.Separators,
				out var parseArgs))
			{
				return ImmutableArray<IImmutableCommand>.Empty;
			}

			var args = parseArgs.Arguments;
			var node = _CommandTrie.Root;
			for (var i = 0; i < args.Count; ++i)
			{
				if (!node.Edges.TryGetValue(args[i], out node))
				{
					break;
				}
				if (i == args.Count - 1)
				{
					return node.Values.ToImmutableArray();
				}
			}
			return ImmutableArray<IImmutableCommand>.Empty;
		}

		public IReadOnlyList<CommandScore> GetCommands(
			IContext context,
			IReadOnlyList<string> input)
		{
			var count = input.Count;
			if (count == 0)
			{
				return Array.Empty<CommandScore>();
			}

			var node = _CommandTrie.Root;
			var matches = new List<CommandScore>();
			for (var i = 0; i < count; ++i)
			{
				foreach (var command in node.Values)
				{
					if (!command.IsValidContext(context))
					{
						matches.Add(CommandScore.FromInvalidContext(command, context, i));
					}

					var (min, max) = GetMinAndMaxArgs(command);
					// Trivial cases, provided input length is not in the possible arg length
					if (count < min)
					{
						matches.Add(CommandScore.FromNotEnoughArgs(command, context, i));
					}
					else if (count > max)
					{
						matches.Add(CommandScore.FromTooManyArgs(command, context, i));
					}
					else
					{
						matches.Add(CommandScore.FromCorrectArgCount(command, context, i));
					}
				}

				if (!node.Edges.TryGetValue(input[i], out node))
				{
					break;
				}
			}
			SortMatches(matches);
			return matches;
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

					arg = trResult.Arg;
					currentIndex += parameter.Length;
				}
				// We don't have any more args to parse.
				// If the parameter isn't optional it's a failure
				else if (!parameter.IsOptional())
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
			var cache = new PreconditionCache(context);
			var matches = new List<CommandScore>();
			for (var i = 0; i < candidates.Count; ++i)
			{
				var candidate = candidates[i];
				// Only allow newly found commands with CorrectArgCount
				// Invalid args counts or any failures means don't check
				if (candidate.Stage != CommandStage.CorrectArgCount
					|| candidate.Command?.IsValidContext(context) != true)
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
			SortMatches(matches);
			return matches;
		}

		public async Task<IResult> ProcessParameterPreconditionsAsync(
			PreconditionCache cache,
			IImmutableCommand command,
			IImmutableParameter parameter,
			object? value)
		{
			var info = new CommandInfo(command, parameter);
			foreach (var precondition in parameter.Preconditions)
			{
				// TODO: enumerables
				var result = await cache.GetResultAsync(info, precondition, value).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		public async Task<IResult> ProcessPreconditionsAsync(
			PreconditionCache cache,
			IImmutableCommand command)
		{
			var info = new CommandInfo(command, null);
			foreach (var precondition in command.Preconditions)
			{
				var result = await cache.GetResultAsync(info, precondition).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		public async Task<ITypeReaderResult> ProcessTypeReadersAsync(
			PreconditionCache cache,
			IImmutableParameter parameter,
			IReadOnlyList<string> input,
			int startIndex)
		{
			// Iterate at least once even for arguments with zero length, i.e. IContext
			// Use length if we're dealing with something we need to make an array out of
			// Otherwise mainly ignore it

			var (makeArray, reader) = GetReader(parameter);
			var pLength = makeArray ? parameter.Length : 1;
			var length = Math.Min(input.Count - startIndex, pLength);
			var results = new List<ITypeReaderResult>(length);

			for (var i = startIndex; i < startIndex + length; ++i)
			{
				var result = await cache.GetResultAsync(reader, input[i]).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
				results.Add(result);
			}

			if (parameter.Length == 0 && results.Count == 0)
			{
				var result = await cache.GetResultAsync(reader, "").ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
				results.Add(result);
			}

			// Return without dealing with the array
			if (!makeArray)
			{
				return results[0];
			}

			// Copy the values from the type reader result list to an array of the parameter type
			var output = Array.CreateInstance(parameter.EnumerableType, results.Count);
			for (var i = 0; i < results.Count; ++i)
			{
				output.SetValue(results[i].Arg, i);
			}
			return TypeReaderResult.FromSuccess(output);
		}

		public void Remove(IImmutableCommand command)
			=> _CommandTrie.Remove(command);

		IReadOnlyList<IImmutableCommand> ICommandService.Find(string input)
			=> Find(input);

		private static (int Min, int Max) GetMinAndMaxArgs(IImmutableCommand command)
		{
			var @base = command.Names[0].Parts.Count;
			var min = @base;
			var max = @base;
			foreach (var parameter in command.Parameters)
			{
				// Remainder will always be the last parameter
				if (parameter.Length == RemainderAttribute.REMAINDER)
				{
					++min;
					max = int.MaxValue;
					break;
				}
				if (!parameter.IsOptional())
				{
					min += parameter.Length;
				}
				max += parameter.Length;
			}
			return (min, max);
		}

		private static void SortMatches(List<CommandScore> matches)
		{
			if (matches.Count > 1)
			{
				// CompareTo backwards since we want the higher values in front
				matches.Sort((x, y) => y.CompareTo(x));
			}
		}

		private (bool, ITypeReader) GetReader(IImmutableParameter parameter)
		{
			// TypeReader is overridden, we /shouldn't/ need to deal with converting
			// to an enumerable for the dev
			if (parameter.OverriddenTypeReader != null)
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
			if (eType != null && _Readers.TryGetReader(eType, out reader))
			{
				return (true, reader);
			}
			throw new ArgumentException($"There is no converter specified for {parameter.ParameterType.Name}.");
		}
	}
}
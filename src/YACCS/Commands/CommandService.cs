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
		private static readonly ImmutableArray<CommandScore> NotFound
			= new[] { CommandScore.FromNotFound() }.ToImmutableArray();
		private static readonly ImmutableArray<CommandScore> QuoteMismatch
			= new[] { CommandScore.FromQuoteMismatch() }.ToImmutableArray();

		private readonly AsyncEvent<CommandExecutedEventArgs> _CommandExecuted
			= new AsyncEvent<CommandExecutedEventArgs>();
		private readonly CommandTrie _CommandTrie;
		private readonly ICommandServiceConfig _Config;
		private readonly ITypeReaderCollection _Readers;

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

		public CommandService(ICommandServiceConfig config, ITypeReaderCollection readers)
		{
			_CommandTrie = new CommandTrie(config.CommandNameComparer);
			_Config = config;
			_Readers = readers;
		}

		public void Add(IImmutableCommand command)
			=> _CommandTrie.Add(command);

		public async Task<IReadOnlyList<CommandScore>> GetBestMatchesAsync(
			IContext context,
			IReadOnlyList<string> input,
			IReadOnlyList<CommandScore> candidates)
		{
			var cache = new PreconditionCache();
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

				matches.Add(await ProcessAllPreconditions(
					cache,
					context,
					input,
					candidate
				).ConfigureAwait(false));
			}
			return GetSortedCommandScores(matches);
		}

		public IReadOnlyList<CommandScore> GetCommands(string input)
		{
			if (ParseArgs.TryParse(
				input,
				_Config.StartQuotes,
				_Config.EndQuotes,
				_Config.Separators,
				out var args))
			{
				return GetCommands(args.Arguments);
			}
			return QuoteMismatch;
		}

		public IReadOnlyList<CommandScore> GetCommands(IReadOnlyList<string> input)
		{
			var count = input.Count;
			if (count == 0)
			{
				return NotFound;
			}

			var node = _CommandTrie.Root;
			var matches = new List<CommandScore>();
			for (var i = 0; i < count; ++i)
			{
				foreach (var command in node.Values)
				{
					var (min, max) = GetMinAndMaxArgs(command);
					// Trivial cases, provided input length is not in the possible arg length
					if (count < min)
					{
						matches.Add(CommandScore.FromNotEnoughArgs(command, i));
					}
					else if (count > max)
					{
						matches.Add(CommandScore.FromTooManyArgs(command, i));
					}
					else
					{
						matches.Add(CommandScore.FromCorrectArgCount(command, i));
					}
				}

				if (!node.Edges.TryGetValue(input[i], out node))
				{
					break;
				}
			}
			return GetSortedCommandScores(matches);
		}

		public Task<CommandScore> ProcessAllPreconditions(
			IContext context,
			IReadOnlyList<string> input,
			CommandScore candidate)
			=> ProcessAllPreconditions(new PreconditionCache(), context, input, candidate);

		public async Task<CommandScore> ProcessAllPreconditions(
			PreconditionCache cache,
			IContext context,
			IReadOnlyList<string> input,
			CommandScore candidate)
		{
			if (candidate.Stage != CommandStage.CorrectArgCount || candidate.Command == null)
			{
				throw new ArgumentException("Invalid stage.", nameof(candidate));
			}

			var command = candidate.Command;
			// Any precondition fails, command is not valid
			var pResult = await ProcessPreconditionsAsync(
				cache,
				context,
				command
			).ConfigureAwait(false);
			if (!pResult.IsSuccess)
			{
				return CommandScore.FromFailedPrecondition(command, context, pResult, 0);
			}

			var args = new object?[command.Parameters.Count];
			var argCount = 0;
			var startIndex = candidate.Score;
			for (var i = 0; i < command.Parameters.Count && startIndex < input.Count; ++i)
			{
				var parameter = command.Parameters[i];

				var trResult = await ProcessTypeReadersAsync(
					cache,
					context,
					parameter,
					input,
					startIndex
				).ConfigureAwait(false);
				if (!trResult.IsSuccess)
				{
					return CommandScore.FromFailedTypeReader(command, context, trResult, i);
				}

				var ppResult = await ProcessParameterPreconditionsAsync(
					cache,
					context,
					parameter,
					trResult.Arg
				).ConfigureAwait(false);
				if (!ppResult.IsSuccess)
				{
					return CommandScore.FromFailedParameterPrecondition(command, context, ppResult, i);
				}

				args[i] = trResult.Arg;
				++argCount;
				startIndex += parameter.Length;
			}

			// Deal with optional parameters if we don't have full arguments
			for (var i = argCount; i < command.Parameters.Count; ++i)
			{
				var parameter = command.Parameters[i];

				if (!parameter.IsOptional())
				{
					return CommandScore.FromFailedOptionalArgs(command, context, i);
				}

				var arg = parameter.DefaultValue;
				var ppResult = await ProcessParameterPreconditionsAsync(
					cache,
					context,
					parameter,
					arg
				).ConfigureAwait(false);
				if (!ppResult.IsSuccess)
				{
					return CommandScore.FromFailedParameterPrecondition(command, context, ppResult, i);
				}

				args[i] = parameter.DefaultValue;
			}
			return CommandScore.FromCanExecute(command, context, args);
		}

		public async Task<IResult> ProcessParameterPreconditionsAsync(
			PreconditionCache cache,
			IContext context,
			IImmutableParameter parameter,
			object? value)
		{
			var ppCache = cache.ParameterPreconditions;
			foreach (var precondition in parameter.Preconditions)
			{
				var key = (value, precondition);
				if (!ppCache.TryGetValue(key, out var result))
				{
					// TODO: enumerables
					result = await precondition.CheckAsync(context, value).ConfigureAwait(false);
					ppCache[key] = result;
				}
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		public async Task<IResult> ProcessPreconditionsAsync(
			PreconditionCache cache,
			IContext context,
			IImmutableCommand candidate)
		{
			var pCache = cache.Preconditions;
			foreach (var precondition in candidate.Preconditions)
			{
				var key = precondition;
				if (!pCache.TryGetValue(key, out var result))
				{
					result = await precondition.CheckAsync(context, candidate).ConfigureAwait(false);
					pCache[key] = result;
				}
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		public async Task<ITypeReaderResult> ProcessTypeReadersAsync(
			PreconditionCache cache,
			IContext context,
			IImmutableParameter parameter,
			IReadOnlyList<string> input,
			int startIndex)
		{
			var (isEnumerable, reader) = GetReader(parameter);
			var length = Math.Min(input.Count, Math.Max(parameter.Length, 1));
			var results = new List<ITypeReaderResult>(length);

			var tCache = cache.TypeReaders;
			// Iterate at least once even for arguments with zero length
			// in cases of IContext, etc
			var endIndex = startIndex + length;
			for (var i = startIndex; i < endIndex; ++i)
			{
				var key = (input[i], parameter.ParameterType);
				if (!tCache.TryGetValue(key, out var result))
				{
					result = await reader.ReadAsync(context, input[i]).ConfigureAwait(false);
					tCache[key] = result;
				}
				if (!result.IsSuccess)
				{
					return result;
				}
				results.Add(result);
			}

			// Length being 1 and not enumerable so return without dealing with the array
			if (parameter.Length == 1 && !isEnumerable)
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

		private static IReadOnlyList<CommandScore> GetSortedCommandScores(List<CommandScore> matches)
		{
			if (matches.Count == 0)
			{
				return NotFound;
			}

			if (matches.Count > 1)
			{
				// CompareTo backwards since we want the higher values in front
				matches.Sort((x, y) => y.CompareTo(x));
			}
			return matches;
		}

		private (bool IsEnumerable, ITypeReader Reader) GetReader(IImmutableParameter parameter)
		{
			if (_Readers.TryGetReader(parameter.ParameterType, out var reader))
			{
				return (false, reader);
			}
			if (parameter.EnumerableType != null
				&& _Readers.TryGetReader(parameter.EnumerableType, out reader))
			{
				return (true, reader);
			}
			throw new ArgumentException($"There is no converter specified for {parameter.ParameterType.Name}.");
		}
	}
}
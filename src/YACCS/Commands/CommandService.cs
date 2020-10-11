using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
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

			if (command.Attributes.Any(x => x is GenerateNamedArgumentsAttribute))
			{
				_CommandTrie.Add(command.GenerateNamedArgumentVersion());
			}
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

			var (result, best) = await GetBestMatchAsync(context, args).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}

			_ = ExecuteCommand(best!, context);
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

		public async Task<(IResult, CommandScore?)> GetBestMatchAsync(
			IContext context,
			IReadOnlyList<string> input)
		{
			var node = _CommandTrie.Root;
			var best = default(CommandScore);
			var cache = new PreconditionCache(context);
			for (var i = 0; i < input.Count; ++i)
			{
				if (!node.TryGetEdge(input[i], out node))
				{
					break;
				}

				foreach (var command in node.Values)
				{
					// Add 1 to i to account for how we're in a node
					var score = await GetCommandScoreAsync(cache, context, command, input, i + 1).ConfigureAwait(false);
					if (_Config.MultiMatchHandling == MultiMatchHandling.Error
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
			IReadOnlyList<string> input,
			int startIndex)
		{
			// Trivial cases, invalid context or arg length
			if (!command.IsValidContext(context.GetType()))
			{
				var score = CommandScore.FromInvalidContext(command, context, startIndex);
				return new ValueTask<CommandScore>(score);
			}
			else if (input.Count < command.MinLength)
			{
				var score = CommandScore.FromNotEnoughArgs(command, context, startIndex);
				return new ValueTask<CommandScore>(score);
			}
			else if (input.Count > command.MaxLength)
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

				var value = parameter.DefaultValue;
				// We still have more args to parse so let's look through those
				if (currentIndex < input.Count)
				{
					var trResult = await ProcessTypeReadersAsync(
						cache,
						parameter,
						input,
						currentIndex
					).ConfigureAwait(false);
					if (!trResult.IsSuccess)
					{
						return CommandScore.FromFailedTypeReader(command, context, trResult, i);
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
				var str = length == 1 ? input[startIndex] : Join(input, startIndex, length);
				return cache.GetResultAsync(reader, str);
			}

			static async Task<ITypeReaderResult> ProcessTypeReadersAsync(
				PreconditionCache cache,
				IImmutableParameter parameter,
				ITypeReader reader,
				IReadOnlyList<string> input,
				int startIndex,
				int length)
			{
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
				return TypeReaderResult<object>.FromSuccess(output);
			}

			return new ValueTask<ITypeReaderResult>(ProcessTypeReadersAsync(
				cache,
				parameter,
				reader,
				input,
				startIndex,
				length
			));
		}

		public void Remove(IImmutableCommand command)
			=> _CommandTrie.Remove(command);

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

		private string Join(IReadOnlyList<string> input, int startIndex, int length)
		{
			var sb = new StringBuilder();
			for (var i = startIndex; i < startIndex + length; ++i)
			{
				if (sb.Length != 0)
				{
					const char Separator = CommandServiceUtils.InternallyUsedSeparator;
					sb.Append(Separator);
				}

				var item = input[i];
				if (item.Contains(_Config.Separator))
				{
					const char Quote = CommandServiceUtils.InternallyUsedQuote;
					sb.Append(Quote).Append(item).Append(Quote);
				}
				else
				{
					sb.Append(item);
				}
			}
			return sb.ToString();
		}
	}
}
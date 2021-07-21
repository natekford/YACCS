using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class ExecutableCommandsTypeReader : CommandsTypeReader
	{
		public override async ITask<ITypeReaderResult<IReadOnlyCollection<IImmutableCommand>>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var result = await base.ReadAsync(context, input).ConfigureAwait(false);
			if (!result.InnerResult.IsSuccess)
			{
				return result;
			}

			var commands = result.Value!;
			var executableCommands = new List<IImmutableCommand>(commands.Count);
			foreach (var command in commands)
			{
				var canExecute = await command.CanExecuteAsync(context).ConfigureAwait(false);
				if (canExecute.IsSuccess)
				{
					executableCommands.Add(command);
				}
			}

			if (executableCommands.Count > 0)
			{
				return TypeReaderResult<IReadOnlyCollection<IImmutableCommand>>.FromSuccess(executableCommands);
			}
			return CachedResults<IReadOnlyCollection<IImmutableCommand>>.ParseFailed;
		}
	}
}
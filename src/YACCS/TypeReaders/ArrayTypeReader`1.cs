using System;
using System.Collections.Generic;
using System.Linq;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.TypeReaders
{
	public class ArrayTypeReader<T> : TypeReader<T[]>
	{
		public override async ITask<ITypeReaderResult<T[]>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var registry = context.Services.GetRequiredService<ITypeRegistry<ITypeReader>>();
			var config = context.Services.GetRequiredService<ICommandServiceConfig>();

			var reader = registry.Get<T>();
			var values = new List<T>(input.Length);
			for (var i = 0; i < input.Length; ++i)
			{
				var iResult = await reader.ReadAsync(context, input.Slice(i, 1)).ConfigureAwait(false);
				if (iResult.InnerResult.IsSuccess)
				{
					values.Add(iResult.Value!);
					continue;
				}

				// Unseparated arguments need separation
				if (!ParseArgs.TryParse(
					input.Span[i],
					config.Separator,
					config.StartQuotes,
					config.EndQuotes,
					out var args) || args.Length < 2)
				{
					return TypeReaderResult<T[]>.FromError(iResult.InnerResult);
				}

				var memory = args.AsMemory();
				for (var j = 0; j < memory.Length; ++j)
				{
					var jResult = await reader.ReadAsync(context, memory.Slice(j, 1)).ConfigureAwait(false);
					if (!jResult.InnerResult.IsSuccess)
					{
						return TypeReaderResult<T[]>.FromError(jResult.InnerResult);
					}
					values.Add(jResult.Value!);
				}
			}
			return TypeReaderResult<T[]>.FromSuccess(values.ToArray());
		}
	}
}
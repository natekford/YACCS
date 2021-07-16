using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.TypeReaders
{
	public class ArrayTypeReader<TValue> : TypeReader<TValue[]>
	{
		public override async ITask<ITypeReaderResult<TValue[]>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			// We don't need to handle having the correct context type because
			// the type reader we retrieve handles that
			var readers = context.Services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();
			var handler = context.Services.GetRequiredService<IArgumentSplitter>();

			var reader = readers.GetTypeReader<TValue>();
			var values = new List<TValue>(input.Length);
			for (var i = 0; i < input.Length; ++i)
			{
				// Try to convert directly from the input string
				var iResult = await reader.ReadAsync(context, input.Slice(i, 1)).ConfigureAwait(false);
				if (iResult.InnerResult.IsSuccess)
				{
					values.Add(iResult.Value!);
					continue;
				}

				// Converting directly from the input string didn't work
				// Try splitting the input string
				if (!handler.TrySplit(input.Span[i], out var args) || args.Length < 2)
				{
					return TypeReaderResult<TValue[]>.FromError(iResult.InnerResult);
				}

				for (var j = 0; j < args.Length; ++j)
				{
					var jResult = await reader.ReadAsync(context, args.Slice(j, 1)).ConfigureAwait(false);
					if (!jResult.InnerResult.IsSuccess)
					{
						return TypeReaderResult<TValue[]>.FromError(jResult.InnerResult);
					}
					values.Add(jResult.Value!);
				}
			}
			return TypeReaderResult<TValue[]>.FromSuccess(values.ToArray());
		}
	}
}
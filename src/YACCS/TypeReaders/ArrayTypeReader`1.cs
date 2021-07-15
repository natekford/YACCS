using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.TypeReaders
{
	public class ArrayTypeReader<T> : TypeReader<T[]>
	{
		public override async ValueTask<ITypeReaderResult<T[]>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var readers = context.Services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();
			var handler = context.Services.GetRequiredService<IArgumentSplitter>();

			var reader = readers[typeof(T)];
			var values = new List<T>(input.Length);
			for (var i = 0; i < input.Length; ++i)
			{
				var iResult = await reader.ReadAsync(context, input.Slice(i, 1)).ConfigureAwait(false);
				if (iResult.InnerResult.IsSuccess)
				{
					values.Add((T)iResult.Value!);
					continue;
				}

				// Unseparated arguments need separation
				if (!handler.TrySplit(input.Span[i], out var args) || args.Length < 2)
				{
					return TypeReaderResult<T[]>.FromError(iResult.InnerResult);
				}

				for (var j = 0; j < args.Length; ++j)
				{
					var jResult = await reader.ReadAsync(context, args.Slice(j, 1)).ConfigureAwait(false);
					if (!jResult.InnerResult.IsSuccess)
					{
						return TypeReaderResult<T[]>.FromError(jResult.InnerResult);
					}
					values.Add((T)jResult.Value!);
				}
			}
			return TypeReaderResult<T[]>.FromSuccess(values.ToArray());
		}
	}
}
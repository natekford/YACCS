using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.TypeReaders
{
	public class ListTypeReader<T> : TypeReader<List<T>>
	{
		public override async ITask<ITypeReaderResult<List<T>>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			// We don't need to handle having the correct context type because
			// the type reader we retrieve handles that
			var readers = context.Services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();
			var handler = context.Services.GetRequiredService<IArgumentHandler>();

			var reader = readers.GetTypeReader<T>();
			var values = new List<T>(input.Length);
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
					return Error(iResult.InnerResult);
				}

				for (var j = 0; j < args.Length; ++j)
				{
					var jResult = await reader.ReadAsync(context, args.Slice(j, 1)).ConfigureAwait(false);
					if (!jResult.InnerResult.IsSuccess)
					{
						return Error(jResult.InnerResult);
					}
					values.Add(jResult.Value!);
				}
			}
			return Success(values);
		}
	}
}
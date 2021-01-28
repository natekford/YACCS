﻿using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class ArrayTypeReader<T> : TypeReader<T[]>
	{
		public override async ITask<ITypeReaderResult<T[]>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var registry = context.Services.GetRequiredService<ITypeRegistry<ITypeReader>>();
			var service = context.Services.GetRequiredService<ICommandService>();

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
				if (!service.TryGetArgs(input.Span[i], out var args) || args.Length < 2)
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
					values.Add(jResult.Value!);
				}
			}
			return TypeReaderResult<T[]>.FromSuccess(values.ToArray());
		}
	}
}
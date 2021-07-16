using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class NullableTypeReader<TValue> : TypeReader<TValue?> where TValue : struct
	{
		private static readonly NullChecker _Checker = new();
		private static readonly ITypeReaderResult<TValue?> _NullResult
			= TypeReaderResult<TValue?>.FromSuccess(null);

		public override async ITask<ITypeReaderResult<TValue?>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			// We don't need to handle having the correct context type because
			// the type reader we retrieve handles that
			var checker = context.Services.GetService<INullChecker>() ?? _Checker;
			if (input.Length == 1 && checker.IsNull(input.Span[0]))
			{
				return _NullResult;
			}

			var readers = context.Services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();
			var reader = readers.GetTypeReader<TValue>();
			var result = await reader.ReadAsync(context, input).ConfigureAwait(false);
			if (!result.InnerResult.IsSuccess)
			{
				return TypeReaderResult<TValue?>.FromError(result.InnerResult);
			}
			return TypeReaderResult<TValue?>.FromSuccess(result.Value);
		}
	}
}
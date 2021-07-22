using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class NullableTypeReader<TValue> : TypeReader<TValue?> where TValue : struct
	{
		private static readonly NullValidator _Null = new();
		private static readonly ITypeReaderResult<TValue?> _NullResult
			= TypeReaderResult<TValue?>.FromSuccess(null);

		public override async ITask<ITypeReaderResult<TValue?>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			// We don't need to handle having the correct context type because
			// the type reader we retrieve handles that
			var @null = context.Services.GetService<INullValidator>() ?? _Null;
			if (@null.IsNull(input!))
			{
				return _NullResult;
			}

			var readers = context.Services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();
			var reader = readers.GetTypeReader<TValue>();
			var result = await reader.ReadAsync(context, input).ConfigureAwait(false);
			// We can't just cast result from TRResult<T> to TRResult<Nullable<T>>
			// So we have to recreate the type reader result
			if (!result.InnerResult.IsSuccess)
			{
				return Error(result.InnerResult);
			}
			return Success(result.Value);
		}
	}
}
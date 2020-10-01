using System;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class NullableTypeReader<T> : TypeReader<T?> where T : struct
	{
		private static readonly ITypeReaderResult<T?> NullResult
			= TypeReaderResult<T?>.FromSuccess(null);
		private readonly ITypeReader<T> _Reader;

		public NullableTypeReader(ITypeReader<T> reader)
		{
			_Reader = reader;
		}

		public override async ITask<ITypeReaderResult<T?>> ReadAsync(IContext context, string input)
		{
			if (input?.Equals("null", StringComparison.OrdinalIgnoreCase) != false)
			{
				return NullResult;
			}

			var result = await _Reader.ReadAsync(context, input).ConfigureAwait(false);
			if (result.IsSuccess)
			{
				return TypeReaderResult<T?>.FromSuccess(result.Value);
			}
			return TypeReaderResult<T?>.Failure.Sync;
		}
	}
}
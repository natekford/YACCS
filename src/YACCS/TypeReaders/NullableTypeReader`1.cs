using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class NullableTypeReader<T> : TypeReader<T?> where T : struct
	{
		private static readonly INullChecker Checker = new NullChecker();
		private static readonly ITypeReaderResult<T?> NullResult
			= TypeReaderResult<T?>.FromSuccess(null);

		private readonly ITypeReader<T> _Reader;

		public NullableTypeReader(ITypeReader<T> reader)
		{
			_Reader = reader;
		}

		public override async ITask<ITypeReaderResult<T?>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var checker = context.Services.GetService<INullChecker>() ?? Checker;
			var item = input.Span[0];
			if (checker.IsNull(item))
			{
				return NullResult;
			}

			var result = await _Reader.ReadAsync(context, input).ConfigureAwait(false);
			if (!result.InnerResult.IsSuccess)
			{
				return TypeReaderResult<T?>.FromError(result.InnerResult);
			}
			return TypeReaderResult<T?>.FromSuccess(result.Value);
		}
	}
}
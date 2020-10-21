using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class StringTypeReader : TypeReader<string?>
	{
		private readonly TypeReaderCacheDelegate<string?> _CacheDelegate = (_, input) =>
		{
			return TypeReaderResult<string>.FromSuccess(input).AsITask();
		};

		public override ITask<ITypeReaderResult<string?>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var cache = context.GetTypeReaderCache();
			return cache.GetAsync(this, context, input.Span[0], _CacheDelegate);
		}
	}
}
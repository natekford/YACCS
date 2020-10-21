using System;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public delegate bool TryParseDelegate<T>(string input, out T result);

	public class TryParseTypeReader<T> : TypeReader<T>
	{
		private readonly TypeReaderCacheDelegate<T> _CacheDelegate;
		private readonly TryParseDelegate<T> _Delegate;

		public TryParseTypeReader(TryParseDelegate<T> @delegate)
		{
			_Delegate = @delegate;
			_CacheDelegate = (_, input) =>
			{
				if (_Delegate(input, out var result))
				{
					return TypeReaderResult<T>.FromSuccess(result).AsITask();
				}
				return TypeReaderResult<T>.Failure.ITask;
			};
		}

		public override ITask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var cache = context.GetTypeReaderCache();
			return cache.GetAsync(this, context, input.Span[0], _CacheDelegate);
		}
	}
}
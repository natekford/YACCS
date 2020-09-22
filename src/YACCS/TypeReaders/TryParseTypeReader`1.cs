using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class TryParseTypeReader<T> : TypeReader<T>
	{
		private readonly TryParseDelegate<T> _Delegate;

		public TryParseTypeReader(TryParseDelegate<T> @delegate)
		{
			_Delegate = @delegate;
		}

		public override Task<ITypeReaderResult<T>> ReadAsync(IContext context, string input)
		{
			if (_Delegate(input, out var result))
			{
				return TypeReaderResult<T>.FromSuccess(result).AsTask();
			}
			return TypeReaderResult<T>.FailureTask;
		}

		public delegate bool TryParseDelegate<TResult>(string input, out TResult result);
	}
}
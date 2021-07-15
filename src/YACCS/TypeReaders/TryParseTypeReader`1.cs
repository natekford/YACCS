using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public delegate bool TryParseDelegate<T>(string input, [MaybeNullWhen(false)] out T result);

	public class TryParseTypeReader<T> : TypeReader<T>
	{
		private readonly TryParseDelegate<T> _Delegate;

		public TryParseTypeReader(TryParseDelegate<T> @delegate)
		{
			_Delegate = @delegate;
		}

		public override ValueTask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (input.Length > 1)
			{
				throw new ArgumentException("Length cannot be more than 1.", nameof(input));
			}
			if (_Delegate(input.Span[0], out var result))
			{
				return new(TypeReaderResult<T>.FromSuccess(result));
			}
			return new(TypeReaderResult<T>.FailureInstance);
		}
	}
}
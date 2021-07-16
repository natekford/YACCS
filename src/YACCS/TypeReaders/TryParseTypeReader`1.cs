using System;
using System.Diagnostics.CodeAnalysis;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public delegate bool TryParseDelegate<TValue>(
		string input,
		[MaybeNullWhen(false)] out TValue result);

	public class TryParseTypeReader<TValue> : TypeReader<TValue>
	{
		private readonly TryParseDelegate<TValue> _Delegate;

		public TryParseTypeReader(TryParseDelegate<TValue> @delegate)
		{
			_Delegate = @delegate;
		}

		public override ITask<ITypeReaderResult<TValue>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (input.Length > 1)
			{
				throw new ArgumentException("Length cannot be more than 1.", nameof(input));
			}
			if (_Delegate(input.Span[0], out var result))
			{
				return TypeReaderResult<TValue>.FromSuccess(result).AsITask();
			}
			return TypeReaderResult<TValue>.FailureInstance;
		}
	}
}
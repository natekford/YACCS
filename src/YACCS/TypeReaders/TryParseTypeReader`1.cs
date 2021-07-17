using System;
using System.Diagnostics.CodeAnalysis;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Parsing;
using YACCS.Results;

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
			var handler = context.Services.GetRequiredService<IArgumentHandler>();
			if (_Delegate(handler.Join(input), out var result))
			{
				return TypeReaderResult<TValue>.FromSuccess(result).AsITask();
			}
			return CachedResults<TValue>.ParseFailedTask;
		}
	}
}
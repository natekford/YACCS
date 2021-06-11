﻿using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public delegate bool TryParseDelegate<T>(string input, out T result);

	public class TryParseTypeReader<T> : TypeReader<T>
	{
		private readonly TryParseDelegate<T> _Delegate;

		public TryParseTypeReader(TryParseDelegate<T> @delegate)
		{
			_Delegate = @delegate;
		}

		public override ITask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (input.Length > 1)
			{
				throw new ArgumentException("Length cannot be more than 1.", nameof(input));
			}
			if (_Delegate(input.Span[0], out var result))
			{
				return TypeReaderResult<T>.FromSuccess(result).AsITask();
			}
			return TypeReaderResult<T>.Failure.ITask;
		}
	}
}
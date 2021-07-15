﻿using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public abstract class TypeReader<T> : ITypeReader<T>
	{
		public Type OutputType => typeof(T);

		public abstract ITask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input);

		ITask<ITypeReaderResult> ITypeReader.ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> ReadAsync(context, input);
	}
}
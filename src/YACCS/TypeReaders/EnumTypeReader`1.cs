using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class EnumTypeReader<TEnum> : TypeReader<TEnum> where TEnum : struct, Enum
	{
		public override ITask<ITypeReaderResult<TEnum>> ReadAsync(IContext context, string input)
		{
			if (Enum.TryParse(input, ignoreCase: true, out TEnum value))
			{
				return TypeReaderResult<TEnum>.FromSuccess(value).AsITask();
			}
			return TypeReaderResult<TEnum>.Failure.ITask;
		}
	}
}
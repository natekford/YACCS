using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class EnumTypeReader<TEnum> : TypeReader<TEnum> where TEnum : struct, Enum
	{
		public override Task<ITypeReaderResult<TEnum>> ReadAsync(IContext context, string input)
		{
			if (Enum.TryParse(input, ignoreCase: true, out TEnum value))
			{
				return TypeReaderResult<TEnum>.FromSuccess(value).AsTask();
			}
			return TypeReaderResult<TEnum>.FailureTask;
		}
	}
}
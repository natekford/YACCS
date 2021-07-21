using System;

namespace YACCS.TypeReaders
{
	public delegate bool EnumDelegate<TEnum>(
		string input,
		bool ignoreCase,
		out TEnum result)
		where TEnum : struct, Enum;

	public class EnumTypeReader<TEnum> : TryParseTypeReader<TEnum> where TEnum : struct, Enum
	{
		public EnumTypeReader() : base(Convert(Enum.TryParse))
		{
		}

		public static TryParseDelegate<TEnum> Convert(EnumDelegate<TEnum> @delegate)
		{
			return (string input, out TEnum result) =>
			{
				const bool IGNORE_CASE = true;
				return @delegate(input, IGNORE_CASE, out result);
			};
		}
	}
}
using System;
using System.Globalization;

namespace YACCS.TypeReaders
{
	public delegate bool NumberDelegate<T>(string input, NumberStyles style, IFormatProvider provider, out T result);

	public class NumberTypeReader<T> : TryParseTypeReader<T>
	{
		public NumberTypeReader(NumberDelegate<T> @delegate) : base(Convert(@delegate))
		{
		}

		public static TryParseDelegate<T> Convert(NumberDelegate<T> @delegate)
		{
			return (string input, out T result) =>
			{
				var provider = CultureInfo.InvariantCulture;
				const NumberStyles STYLE = NumberStyles.Integer;
				return @delegate(input, STYLE, provider, out result);
			};
		}
	}
}
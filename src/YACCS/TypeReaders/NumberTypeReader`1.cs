using System;
using System.Globalization;

namespace YACCS.TypeReaders
{
	public class NumberTypeReader<T> : TryParseTypeReader<T>
	{
		public NumberTypeReader(NumberDelegate @delegate) : base(Convert(@delegate))
		{
		}

		public static TryParseDelegate<T> Convert(NumberDelegate @delegate)
		{
			return (string input, out T result) =>
			{
				var provider = CultureInfo.InvariantCulture;
				const NumberStyles STYLE = NumberStyles.Integer;
				return @delegate(input, STYLE, provider, out result);
			};
		}

		public delegate bool NumberDelegate(string input, NumberStyles style, IFormatProvider provider, out T result);
	}
}
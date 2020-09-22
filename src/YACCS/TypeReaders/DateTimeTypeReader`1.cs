using System;
using System.Globalization;

namespace YACCS.TypeReaders
{
	public class DateTimeTypeReader<T> : TryParseTypeReader<T>
	{
		public DateTimeTypeReader(DateTimeDelegate @delegate) : base(Convert(@delegate))
		{
		}

		public static TryParseDelegate<T> Convert(DateTimeDelegate @delegate)
		{
			return (string input, out T result) =>
			{
				var provider = CultureInfo.InvariantCulture;
				const DateTimeStyles STYLE = DateTimeStyles.None;
				return @delegate(input, provider, STYLE, out result);
			};
		}

		public delegate bool DateTimeDelegate(string input, IFormatProvider provider, DateTimeStyles style, out T result);
	}
}
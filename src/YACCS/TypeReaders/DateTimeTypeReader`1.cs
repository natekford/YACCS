using System;
using System.Globalization;

namespace YACCS.TypeReaders
{
	public delegate bool DateTimeDelegate<T>(
		string input,
		IFormatProvider provider,
		DateTimeStyles style,
		out T result);

	public class DateTimeTypeReader<T> : TryParseTypeReader<T>
	{
		public DateTimeTypeReader(DateTimeDelegate<T> @delegate) : base(Convert(@delegate))
		{
		}

		public static TryParseDelegate<T> Convert(DateTimeDelegate<T> @delegate)
		{
			return (string input, out T result) =>
			{
				var provider = CultureInfo.InvariantCulture;
				const DateTimeStyles STYLE = DateTimeStyles.None;
				return @delegate(input, provider, STYLE, out result);
			};
		}
	}
}
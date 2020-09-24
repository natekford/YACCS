using System;
using System.Globalization;

namespace YACCS.TypeReaders
{
	public delegate bool TimeSpanDelegate<T>(string input, IFormatProvider provider, out T result);

	public class TimeSpanTypeReader<T> : TryParseTypeReader<T>
	{
		public TimeSpanTypeReader(TimeSpanDelegate<T> @delegate) : base(Convert(@delegate))
		{
		}

		public static TryParseDelegate<T> Convert(TimeSpanDelegate<T> @delegate)
		{
			return (string input, out T result) =>
			{
				var provider = CultureInfo.InvariantCulture;
				return @delegate(input, provider, out result);
			};
		}
	}
}
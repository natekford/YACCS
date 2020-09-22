using System;
using System.Globalization;

namespace YACCS.TypeReaders
{
	public class TimeSpanTypeReader<T> : TryParseTypeReader<T>
	{
		public TimeSpanTypeReader(TimeSpanDelegate @delegate) : base(Convert(@delegate))
		{
		}

		public static TryParseDelegate<T> Convert(TimeSpanDelegate @delegate)
		{
			return (string input, out T result) =>
			{
				var provider = CultureInfo.InvariantCulture;
				return @delegate(input, provider, out result);
			};
		}

		public delegate bool TimeSpanDelegate(string input, IFormatProvider provider, out T result);
	}
}
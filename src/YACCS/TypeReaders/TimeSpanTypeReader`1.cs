using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace YACCS.TypeReaders;

/// <inheritdoc cref="TimeSpan.TryParse(string, IFormatProvider, out TimeSpan)"/>
public delegate bool TimeSpanDelegate<T>(
	string input,
	IFormatProvider provider,
	out T result);

/// <summary>
/// Parses a <typeparamref name="T"/> via <see cref="TimeSpanDelegate{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="delegate">The delegate to use when parsing.</param>
public class TimeSpanTypeReader<T>(TimeSpanDelegate<T> @delegate)
	: TryParseTypeReader<T>(Convert(@delegate))
{
	private static TryParseDelegate<T> Convert(TimeSpanDelegate<T> @delegate)
	{
		return (string input, [MaybeNullWhen(false)] out T result) =>
		{
			var provider = CultureInfo.CurrentCulture;
			return @delegate(input, provider, out result);
		};
	}
}
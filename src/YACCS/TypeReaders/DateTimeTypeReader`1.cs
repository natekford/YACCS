using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace YACCS.TypeReaders;

/// <inheritdoc cref="DateTime.TryParse(string, IFormatProvider, DateTimeStyles, out DateTime)"/>
public delegate bool DateTimeDelegate<T>(
	string input,
	IFormatProvider provider,
	DateTimeStyles style,
	out T result);

/// <summary>
/// Parses a <typeparamref name="T"/> via <see cref="DateTimeDelegate{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="delegate">The delegate to use when parsing.</param>
public class DateTimeTypeReader<T>(DateTimeDelegate<T> @delegate)
	: TryParseTypeReader<T>(Convert(@delegate))
{
	private static TryParseDelegate<T> Convert(DateTimeDelegate<T> @delegate)
	{
		return (string input, [MaybeNullWhen(false)] out T result) =>
		{
			var provider = CultureInfo.CurrentCulture;
			const DateTimeStyles STYLE = DateTimeStyles.None;
			return @delegate(input, provider, STYLE, out result);
		};
	}
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace YACCS.TypeReaders;

/// <inheritdoc cref="int.TryParse(string, NumberStyles, IFormatProvider, out int)"/>
public delegate bool NumberDelegate<T>(
	string input,
	NumberStyles style,
	IFormatProvider provider,
	out T result);

/// <summary>
/// Parses a <typeparamref name="T"/> via <see cref="NumberDelegate{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Creates a new <see cref="NumberTypeReader{T}"/>.
/// </remarks>
/// <param name="delegate">The delegate to use when parsing.</param>
public class NumberTypeReader<T>(NumberDelegate<T> @delegate)
	: TryParseTypeReader<T>(Convert(@delegate))
{
	private static TryParseDelegate<T> Convert(NumberDelegate<T> @delegate)
	{
		return (string input, [MaybeNullWhen(false)] out T result) =>
		{
			var provider = CultureInfo.CurrentCulture;
			const NumberStyles STYLE = NumberStyles.Integer | NumberStyles.AllowDecimalPoint;
			return @delegate(input, STYLE, provider, out result);
		};
	}
}
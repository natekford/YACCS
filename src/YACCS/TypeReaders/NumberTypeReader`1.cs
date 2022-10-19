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
public class NumberTypeReader<T> : TryParseTypeReader<T>
{
	/// <summary>
	/// Creates a new <see cref="NumberTypeReader{T}"/>.
	/// </summary>
	/// <param name="delegate">The delegate to use when parsing.</param>
	public NumberTypeReader(NumberDelegate<T> @delegate) : base(Convert(@delegate))
	{
	}

	private static TryParseDelegate<T> Convert(NumberDelegate<T> @delegate)
	{
		return (string input, [MaybeNullWhen(false)] out T result) =>
		{
			var provider = CultureInfo.CurrentCulture;
			const NumberStyles STYLE = NumberStyles.Number;
			return @delegate(input, STYLE, provider, out result);
		};
	}
}
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
public class DateTimeTypeReader<T> : TryParseTypeReader<T>
{
	/// <summary>
	/// Creates a new <see cref="DateTimeTypeReader{T}"/>.
	/// </summary>
	/// <param name="delegate">The delegate to use when parsing.</param>
	public DateTimeTypeReader(DateTimeDelegate<T> @delegate) : base(Convert(@delegate))
	{
	}

	private static TryParseDelegate<T> Convert(DateTimeDelegate<T> @delegate)
	{
		return (string input, out T result) =>
		{
			var provider = CultureInfo.CurrentCulture;
			const DateTimeStyles STYLE = DateTimeStyles.None;
			return @delegate(input, provider, STYLE, out result);
		};
	}
}
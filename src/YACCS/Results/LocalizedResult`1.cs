using YACCS.Localization;

namespace YACCS.Results;

/// <summary>
/// The base class of a result which can be localized and must format arguments in
/// some way at runtime.
/// </summary>
/// <remarks>
/// Creates a new <see cref="LocalizedResult{T}"/>.
/// </remarks>
/// <inheritdoc cref="Result(bool, string)"/>
public class LocalizedResult<T>(
	bool isSuccess,
	NeedsLocalization response,
	T value
) : LocalizedResult(isSuccess, response), IFormattable
{
	/// <inheritdoc />
	public override string Response => ToString(null, null);
	/// <summary>
	/// The value to include in the response.
	/// </summary>
	public T Value { get; } = value;
	/// <summary>
	/// The format to use for creating a response.
	/// </summary>
	protected virtual string Format => UnlocalizedResponse.Localized;

	/// <inheritdoc />
	public virtual string ToString(string? format, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Value);
}
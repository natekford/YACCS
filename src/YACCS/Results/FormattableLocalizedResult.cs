using YACCS.Localization;

namespace YACCS.Results;

/// <summary>
/// The base class of a result which can be localized and must format arguments in
/// some way at runtime.
/// </summary>
public abstract class FormattableLocalizedResult : LocalizedResult, IFormattable
{
	/// <inheritdoc />
	public override string Response => ToString(null, null);

	/// <summary>
	/// Creates a new <see cref="FormattableLocalizedResult"/>.
	/// </summary>
	/// <inheritdoc cref="Result(bool, string)"/>
	protected FormattableLocalizedResult(bool isSuccess, NeedsLocalization response)
		: base(isSuccess, response)
	{
	}

	/// <inheritdoc />
	public abstract string ToString(string? format, IFormatProvider? formatProvider);
}
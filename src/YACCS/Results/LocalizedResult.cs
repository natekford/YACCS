using YACCS.Localization;

namespace YACCS.Results;

/// <summary>
/// The base class of a result which can be localized.
/// </summary>
public abstract class LocalizedResult : Result
{
	/// <inheritdoc />
	public override string Response => UnlocalizedResponse.Localized;
	/// <summary>
	/// The unlocalized text representing this result.
	/// </summary>
	protected NeedsLocalization UnlocalizedResponse { get; }

	/// <summary>
	/// Creates a new <see cref="LocalizedResult"/>.
	/// </summary>
	/// <inheritdoc cref="Result(bool, string)"/>
	protected LocalizedResult(bool isSuccess, NeedsLocalization response)
		: base(isSuccess, "")
	{
		UnlocalizedResponse = response;
	}
}
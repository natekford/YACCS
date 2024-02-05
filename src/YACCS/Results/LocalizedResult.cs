using YACCS.Localization;

namespace YACCS.Results;

/// <summary>
/// The base class of a result which can be localized.
/// </summary>
/// <inheritdoc cref="Result(bool, string)"/>
public class LocalizedResult(bool isSuccess, NeedsLocalization response)
	: Result(isSuccess, "")
{
	/// <inheritdoc />
	public override string Response => UnlocalizedResponse.Localized;
	/// <summary>
	/// The unlocalized text representing this result.
	/// </summary>
	protected NeedsLocalization UnlocalizedResponse { get; } = response;
}
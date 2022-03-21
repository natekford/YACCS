using YACCS.Localization;

namespace YACCS.Results;

/// <summary>
/// The base class of a result which can be localized and should only have a singleton
/// instance.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SingletonLocalizedResult<T> : LocalizedResult
	where T : LocalizedResult, new()
{
	/// <summary>
	/// A singleton instance of this result.
	/// </summary>
	public static T Instance { get; } = new();

	/// <summary>
	/// Creates a new <see cref="SingletonLocalizedResult{T}"/>.
	/// </summary>
	/// <inheritdoc cref="Result(bool, string)"/>
	protected SingletonLocalizedResult(bool isSuccess, NeedsLocalization response)
		: base(isSuccess, response)
	{
	}
}

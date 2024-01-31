using System.Globalization;
using System.Resources;

namespace YACCS.Localization;

/// <summary>
/// Wrapper for a <see cref="ResourceManager"/>.
/// </summary>
/// <remarks>
/// Creates a new <see cref="ResourceManagerLocalizer"/>.
/// </remarks>
/// <param name="resourceManager">The resource manager to use for localization.</param>
public sealed class ResourceManagerLocalizer(ResourceManager resourceManager)
	: ILocalizer
{
	private readonly ResourceManager _ResourceManager = resourceManager;

	/// <inheritdoc />
	public string? Get(string key, CultureInfo? culture = null)
		=> _ResourceManager.GetString(key, culture);
}
using System.Globalization;
using System.Resources;

namespace YACCS.Localization
{
	/// <summary>
	/// Wrapper for a <see cref="ResourceManager"/>.
	/// </summary>
	public sealed class ResourceManagerLocalizer : ILocalizer
	{
		private readonly ResourceManager _ResourceManager;

		/// <summary>
		/// Creates a new <see cref="ResourceManagerLocalizer"/>.
		/// </summary>
		/// <param name="resourceManager">The resource manager to use for localization.</param>
		public ResourceManagerLocalizer(ResourceManager resourceManager)
		{
			_ResourceManager = resourceManager;
		}

		/// <inheritdoc />
		public string? Get(string key, CultureInfo? culture = null)
			=> _ResourceManager.GetString(key, culture);
	}
}
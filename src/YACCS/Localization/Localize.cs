namespace YACCS.Localization
{
	/// <summary>
	/// Utilities for localization.
	/// </summary>
	public static class Localize
	{
		/// <summary>
		/// A singleton instance of <see cref="AggregateLocalizer"/>.
		/// </summary>
		public static AggregateLocalizer Instance { get; } = new();

		/// <summary>
		/// Localizes <paramref name="key"/>.
		/// </summary>
		/// <param name="localizer">The localizer to find the key in.</param>
		/// <param name="key">The key to search for.</param>
		/// <param name="fallback">The fallback value if the key is not found.</param>
		/// <returns>A localized string, or the fallback, or the key.</returns>
		public static string GetSafely(this ILocalizer? localizer, string key, string? fallback = null)
			=> localizer?.Get(key) ?? fallback ?? key;

		/// <summary>
		/// Calls <see cref="GetSafely(ILocalizer?, string, string?)"/> with
		/// <see cref="Instance"/>.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="fallback">The fallback value if the key is not found.</param>
		/// <returns>A localized string, or the fallback, or the key.</returns>
		public static string This(string key, string? fallback = null)
			=> Instance.GetSafely(key, fallback);
	}
}
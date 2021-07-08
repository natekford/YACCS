namespace YACCS.Localization
{
	public static class Localize
	{
		public static AggregateLocalizer Instance { get; } = new();

		public static string GetSafely(this ILocalizer? localizer, string key, string? fallback = null)
			=> localizer?.Get(key) ?? fallback ?? key;

		public static string This(string key, string? fallback = null)
			=> Instance.GetSafely(key, fallback);
	}
}
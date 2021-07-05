namespace YACCS.Localization
{
	public static class Localize
	{
		public static AggregateLocalizer Instance { get; } = new();

		public static string GetSafely(this ILocalizer? localizer, string key, string? @default = null)
			=> localizer?.Get(key) ?? @default ?? key;

		public static string This(string key, string? @default = null)
			=> Instance.GetSafely(key, @default);
	}
}
namespace YACCS.Localization
{
	public sealed class NeedsLocalization
	{
		public string? Fallback { get; }
		public string Key { get; }
		public string Localized => Localize.This(Key, Fallback);

		public NeedsLocalization(string key, string? fallback = null)
		{
			Key = key;
			Fallback = fallback;
		}

		public static implicit operator string(NeedsLocalization localized)
			=> localized.ToString();

		public override string ToString()
			=> Localized;
	}
}
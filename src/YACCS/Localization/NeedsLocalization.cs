namespace YACCS.Localization
{
	public sealed class NeedsLocalization
	{
		public string Key { get; }

		public NeedsLocalization(string key)
		{
			Key = key;
		}

		public static implicit operator NeedsLocalization(string key)
			=> new(key);

		public override string ToString()
			=> Localize.This(Key);
	}
}
namespace YACCS.Localization
{
	/// <summary>
	/// Localizes a string or displays a fallback.
	/// </summary>
	public sealed class NeedsLocalization
	{
		/// <summary>
		/// The value to use when <see cref="Key"/> is not registered
		/// in <see cref="Localize.Instance"/>.
		/// </summary>
		public string? Fallback { get; }
		/// <summary>
		/// The key to search for in <see cref="Localize.Instance"/>.
		/// </summary>
		public string Key { get; }
		/// <summary>
		/// Calls <see cref="Localize.This(string, string?)"/>.
		/// </summary>
		public string Localized => Localize.This(Key, Fallback);

		/// <summary>
		/// Creates a new <see cref="NeedsLocalization"/> and sets
		/// <see cref="Fallback"/> to <paramref name="fallback"/> and
		/// <see cref="Key"/> to <paramref name="key"/>.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="fallback"></param>
		public NeedsLocalization(string key, string? fallback = null)
		{
			Key = key;
			Fallback = fallback;
		}

		/// <inheritdoc cref="ToString" />
		public static implicit operator string(NeedsLocalization localized)
			=> localized.ToString();

		/// <inheritdoc />
		public override string ToString()
			=> Localized;
	}
}
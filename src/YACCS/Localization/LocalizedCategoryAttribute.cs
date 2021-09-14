
using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	/// <inheritdoc />
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class LocalizedCategoryAttribute : CategoryAttribute
	{
		/// <inheritdoc />
		public override string Category => Localize.This(Key);
		/// <summary>
		/// The key for localization.
		/// </summary>
		public string Key { get; }

		/// <summary>
		/// Creates a new <see cref="LocalizedCategoryAttribute"/>.
		/// </summary>
		/// <param name="key">
		/// <inheritdoc cref="Key" path="/summary"/>
		/// </param>
		public LocalizedCategoryAttribute(string key) : base(key)
		{
			Key = key;
		}
	}
}
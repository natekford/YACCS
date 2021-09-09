using System;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	/// <inheritdoc/>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class LocalizedNameAttribute : NameAttribute
	{
		/// <summary>
		/// The key for localization.
		/// </summary>
		public string Key { get; }
		/// <inheritdoc/>
		public override string Name => Localize.This(Key);

		/// <summary>
		/// Creates a new <see cref="LocalizedNameAttribute"/>.
		/// </summary>
		/// <param name="key">
		/// <inheritdoc cref="Key" path="/summary"/>
		/// </param>
		public LocalizedNameAttribute(string key) : base(key)
		{
			Key = key;
		}
	}
}
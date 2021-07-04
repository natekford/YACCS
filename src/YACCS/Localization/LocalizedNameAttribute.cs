using System;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class LocalizedNameAttribute : NameAttribute, IUsesLocalizer
	{
		public string Key { get; }
		public virtual ILocalizer? Localizer { get; set; }
		public override string Name => Localizer?.Get(Key) ?? base.Name;

		public LocalizedNameAttribute(string key) : base(key)
		{
			Key = key;
		}
	}
}
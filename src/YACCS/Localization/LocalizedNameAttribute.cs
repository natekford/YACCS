using System;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class LocalizedNameAttribute : NameAttribute
	{
		public string Key { get; }
		public override string Name => Localize.This(Key, base.Name);

		public LocalizedNameAttribute(string key) : base(key)
		{
			Key = key;
		}
	}
}
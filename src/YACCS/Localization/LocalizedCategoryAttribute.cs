using System;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class LocalizedCategoryAttribute : CategoryAttribute
	{
		public override string Category => Localize.This(Key);
		public string Key { get; }

		public LocalizedCategoryAttribute(string key) : base(key)
		{
			Key = key;
		}
	}
}
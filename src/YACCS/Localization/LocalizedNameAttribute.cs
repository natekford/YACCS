using System;
using System.Resources;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public abstract class LocalizedNameAttribute : NameAttribute, IUsesResourceManager
	{
		public string Key { get; }
		public override string Name => ResourceManager.GetString(Key) ?? Key;
		public abstract ResourceManager ResourceManager { get; }

		protected LocalizedNameAttribute(string key) : base(key)
		{
			Key = key;
		}
	}
}
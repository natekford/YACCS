using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Resources;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	[AttributeUsage(AttributeTargets.Class | AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public abstract class LocalizedCommandAttribute : CommandAttribute, IUsesResourceManager
	{
		public IReadOnlyList<string> Keys { get; }
		public override IReadOnlyList<string> Names
		{
			get
			{
				var names = new string[Keys.Count];
				for (var i = 0; i < names.Length; ++i)
				{
					names[i] = ResourceManager.GetString(Keys[i]) ?? Keys[i];
				}
				return names;
			}
		}
		public abstract ResourceManager ResourceManager { get; }

		protected LocalizedCommandAttribute(params string[] keys) : this(keys.ToImmutableArray())
		{
		}

		protected LocalizedCommandAttribute(IReadOnlyList<string> keys) : base(keys)
		{
			Keys = keys;
		}

		protected LocalizedCommandAttribute() : this(ImmutableArray<string>.Empty)
		{
		}
	}
}
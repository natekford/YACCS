using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	[AttributeUsage(AttributeTargets.Class | AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class LocalizedCommandAttribute : CommandAttribute
	{
		public IReadOnlyList<string> Keys { get; }
		public override IReadOnlyList<string> Names
		{
			get
			{
				if (Localize.Instance is null)
				{
					return base.Names;
				}

				var names = ImmutableArray.CreateBuilder<string>(Keys.Count);
				for (var i = 0; i < names.Count; ++i)
				{
					names.Add(Localize.This(Keys[i]));
				}
				return names.MoveToImmutable();
			}
		}

		public LocalizedCommandAttribute(params string[] keys) : this(keys.ToImmutableArray())
		{
		}

		public LocalizedCommandAttribute(IReadOnlyList<string> keys) : base(keys)
		{
			Keys = keys;
		}

		public LocalizedCommandAttribute() : this(ImmutableArray<string>.Empty)
		{
		}
	}
}
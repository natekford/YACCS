﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	[AttributeUsage(AttributeTargets.Class | AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class LocalizedCommandAttribute : CommandAttribute, IUsesLocalizer
	{
		public IReadOnlyList<string> Keys { get; }
		public virtual ILocalizer? Localizer { get; set; }
		public override IReadOnlyList<string> Names
		{
			get
			{
				if (Localizer is null)
				{
					return base.Names;
				}

				var names = ImmutableArray.CreateBuilder<string>(Keys.Count);
				for (var i = 0; i < names.Count; ++i)
				{
					names.Add(Localizer.Get(Keys[i]) ?? Keys[i]);
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
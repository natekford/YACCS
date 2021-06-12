using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class CommandAttribute : Attribute, ICommandAttribute
	{
		public bool AllowInheritance { get; set; }
		public virtual IReadOnlyList<string> Names { get; }

		public CommandAttribute(params string[] names) : this(names.ToImmutableArray())
		{
		}

		public CommandAttribute(IReadOnlyList<string> names)
		{
			Names = names;
		}

		public CommandAttribute() : this(ImmutableArray<string>.Empty)
		{
		}
	}
}
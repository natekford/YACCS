using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class CommandAttribute : Attribute, ICommandAttribute
	{
		public bool AllowInheritance { get; set; }
		public IReadOnlyList<string> Names { get; }

		public CommandAttribute(params string[] names)
		{
			Names = names.ToImmutableArray();
		}

		public CommandAttribute() : this(Array.Empty<string>())
		{
		}
	}
}
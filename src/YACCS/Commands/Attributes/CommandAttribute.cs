using System;
using System.Collections.Generic;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class CommandAttribute : Attribute, ICommandAttribute
	{
		public bool AllowInheritance { get; set; }
		public IReadOnlyList<string> Names { get; }

		public CommandAttribute(params string[] names)
		{
			Names = names;
		}

		public CommandAttribute() : this(Array.Empty<string>())
		{
		}
	}
}
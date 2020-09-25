using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class CommandAttribute : Attribute, ICommandAttribute
	{
		public bool AllowInheritance { get; set; }
		public IReadOnlyList<string> Names { get; }

		public CommandAttribute(params string[] names)
		{
			var builder = ImmutableArray.CreateBuilder<string>(names.Length);
			foreach (var name in names)
			{
				if (name.Contains(' '))
				{
					throw new ArgumentException("Command names cannot contain spaces.", nameof(names));
				}
				builder.Add(name);
			}
			Names = builder.MoveToImmutable();
		}

		public CommandAttribute() : this(Array.Empty<string>())
		{
		}
	}
}
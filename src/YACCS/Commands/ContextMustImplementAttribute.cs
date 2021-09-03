using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Attributes;

namespace YACCS.Commands
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
	public class ContextMustImplementAttribute : Attribute, IContextConstraint
	{
		public virtual IReadOnlyList<Type> Types { get; }

		public ContextMustImplementAttribute(params Type[] types) : this(types.ToImmutableArray())
		{
		}

		public ContextMustImplementAttribute(IReadOnlyList<Type> types)
		{
			Types = types;
		}

		public bool DoesTypeSatisfy(Type type)
		{
			foreach (var constraint in Types)
			{
				if (!constraint.IsAssignableFrom(type))
				{
					return false;
				}
			}
			return true;
		}
	}
}
using System;
using System.Collections.Generic;

using YACCS.Help;
using YACCS.Help.Attributes;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class PriorityAttribute : Attribute, IPriorityAttribute, IRuntimeFormattableAttribute
	{
		public int Priority { get; }

		public PriorityAttribute(int priority)
		{
			Priority = priority;
		}

		public virtual IReadOnlyList<TaggedString> Format(IContext context)
		{
			return new TaggedString[]
			{
				new(Tag.Key, "Priority"),
				new(Tag.Value, Priority.ToString()),
			};
		}
	}
}
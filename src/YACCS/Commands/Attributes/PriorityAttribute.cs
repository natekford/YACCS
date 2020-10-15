using System;
using System.Collections.Generic;

using YACCS.Help;
using YACCS.Help.Attributes;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class PriorityAttribute : Attribute, IPriorityAttribute, IRuntimeFormattableAttribute
	{
		private static readonly TaggedString _Key = new TaggedString(Tag.Key, "Priority");

		public int Priority { get; }

		public PriorityAttribute(int priority)
		{
			Priority = priority;
		}

		public virtual IReadOnlyList<TaggedString> Format(IContext context)
		{
			return new[]
			{
				_Key,
				new TaggedString(Tag.Value, Priority.ToString()),
			};
		}
	}
}
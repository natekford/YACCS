using System;
using System.Collections.Generic;

using YACCS.Help;
using YACCS.Help.Attributes;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class IdAttribute : Attribute, IIdAttribute, IRuntimeFormattableAttribute
	{
		public virtual string Id { get; }

		public IdAttribute(string id)
		{
			Id = id;
		}

		public virtual IReadOnlyList<TaggedString> Format(IContext context)
		{
			return new TaggedString[]
			{
				new(Tag.Key, "Id"),
				new(Tag.Value, Id),
			};
		}
	}
}
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
		protected virtual string IdString { get; } = "Id";

		public IdAttribute(string id)
		{
			Id = id;
		}

		public virtual IReadOnlyList<TaggedString> Format(IContext context)
		{
			return new TaggedString[]
			{
				new(Tag.Key, IdString),
				new(Tag.Value, Id),
			};
		}
	}
}
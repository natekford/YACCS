using System;
using System.Collections.Generic;

using YACCS.Help;
using YACCS.Help.Attributes;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class IdAttribute : Attribute, IIdAttribute, IRuntimeFormattableAttribute
	{
		private static readonly TaggedString _Key = new(Tag.Key, "Id");

		public string Id { get; }

		public IdAttribute(string id)
		{
			Id = id;
		}

		public virtual IReadOnlyList<TaggedString> Format(IContext context)
		{
			return new[]
			{
				_Key,
				new TaggedString(Tag.Value, Id),
			};
		}
	}
}
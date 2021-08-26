using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class CategoryAttribute : Attribute, ICategoryAttribute
	{
		public virtual string Category { get; }

		public CategoryAttribute(string category)
		{
			Category = category;
		}
	}
}
using System;

namespace YACCS.Commands.Attributes
{
	/// <inheritdoc cref="ICategoryAttribute"/>
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
	public class CategoryAttribute : Attribute, ICategoryAttribute
	{
		/// <inheritdoc />
		public virtual string Category { get; }

		/// <summary>
		/// Creates a new <see cref="CategoryAttribute"/> and sets <see cref="Category"/>
		/// to <paramref name="category"/>.
		/// </summary>
		/// <param name="category"></param>
		public CategoryAttribute(string category)
		{
			Category = category;
		}
	}
}
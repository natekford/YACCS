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
		/// Creates a new <see cref="CategoryAttribute"/>.
		/// </summary>
		/// <param name="category">
		/// <inheritdoc cref="Category" path="/summary"/>
		/// </param>
		public CategoryAttribute(string category)
		{
			Category = category;
		}
	}
}
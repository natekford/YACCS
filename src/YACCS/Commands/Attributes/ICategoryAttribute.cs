using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{
	/// <summary>
	/// An attribute which defines a category in <see cref="IQueryableEntity.Attributes"/>.
	/// </summary>
	public interface ICategoryAttribute
	{
		/// <summary>
		/// The name of the category.
		/// </summary>
		string Category { get; }
	}
}
namespace YACCS.Commands.Building
{
	/// <summary>
	/// Defines a method for determining if a context is valid.
	/// </summary>
	public interface IContextConstraint
	{
		/// <summary>
		/// Determines if <paramref name="context"/> is valid.
		/// </summary>
		/// <param name="context">The context to test.</param>
		/// <returns>A bool indicating success or failure.</returns>
		bool DoesTypeSatisfy(Type context);
	}
}
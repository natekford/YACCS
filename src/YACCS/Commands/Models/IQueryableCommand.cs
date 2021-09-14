namespace YACCS.Commands.Models
{
	/// <summary>
	/// A command which supports querying.
	/// </summary>
	public interface IQueryableCommand : IQueryableEntity
	{
		/// <summary>
		/// The type of context required for this command.
		/// </summary>
		Type ContextType { get; }
		/// <summary>
		/// The full paths that lead to this command.
		/// </summary>
		IEnumerable<IReadOnlyList<string>> Paths { get; }
		/// <summary>
		/// The parameters this command requires.
		/// </summary>
		IReadOnlyList<IQueryableParameter> Parameters { get; }
		/// <summary>
		/// The source of this command if this instance is generated.
		/// </summary>
		IImmutableCommand? Source { get; }

		/// <summary>
		/// Determines whether or not <paramref name="type"/> can be
		/// used in this command.
		/// </summary>
		/// <param name="type">The type to determine is valid.</param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="type"/> is a valid context.
		/// </returns>
		bool IsValidContext(Type type);
	}
}
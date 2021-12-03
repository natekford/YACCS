namespace YACCS.Commands.Models;

/// <summary>
/// A mutable command.
/// </summary>
public interface ICommand : IEntityBase, IQueryableCommand
{
	/// <inheritdoc cref="IQueryableCommand.Parameters"/>
	new IReadOnlyList<IParameter> Parameters { get; }
	/// <inheritdoc cref="IQueryableCommand.Paths"/>
	new IList<IReadOnlyList<string>> Paths { get; set; }

	/// <summary>
	/// Creates a new <see cref="IImmutableCommand"/>.
	/// </summary>
	/// <returns>The immutable command.</returns>
	IImmutableCommand ToImmutable();

	/// <summary>
	/// Creates a new <see cref="IImmutableCommand"/> and then allows
	/// any attributes to generate more commands.
	/// </summary>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <returns>The immutable command and any generated commands.</returns>
	IAsyncEnumerable<IImmutableCommand> ToMultipleImmutableAsync(IServiceProvider services);
}

using System;
using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	/// <summary>
	/// A mutable command.
	/// </summary>
	public interface ICommand : IEntityBase, IQueryableCommand
	{
		/// <inheritdoc cref="IQueryableCommand.Names"/>
		new IList<IReadOnlyList<string>> Names { get; set; }
		/// <inheritdoc cref="IQueryableCommand.Parameters"/>
		new IReadOnlyList<IParameter> Parameters { get; }

		/// <summary>
		/// Creates a new <see cref="IImmutableCommand"/>.
		/// </summary>
		/// <returns></returns>
		IImmutableCommand ToImmutable();

		/// <summary>
		/// Creates a new <see cref="IImmutableCommand"/> and then allows
		/// any attributes to generate more commands.
		/// </summary>
		/// <param name="services">The services to use for dependency injection.</param>
		/// <returns></returns>
		IAsyncEnumerable<IImmutableCommand> ToMultipleImmutableAsync(IServiceProvider services);
	}
}
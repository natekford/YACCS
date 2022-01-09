using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models;

/// <summary>
/// An immutable command.
/// </summary>
public interface IImmutableCommand : IImmutableEntityBase, IQueryableCommand
{
	/// <summary>
	/// Whether or not this command should show up for help commands.
	/// </summary>
	bool IsHidden { get; }
	/// <summary>
	/// The maximum possible length for all the parameters added together.
	/// </summary>
	int MaxLength { get; }
	/// <summary>
	/// The minimum possible length for all the parameters added together.
	/// </summary>
	int MinLength { get; }
	/// <inheritdoc cref="IQueryableCommand.Parameters"/>
	new IReadOnlyList<IImmutableParameter> Parameters { get; }
	/// <inheritdoc cref="IQueryableCommand.Paths"/>
	new IReadOnlyList<IReadOnlyList<string>> Paths { get; }
	/// <summary>
	/// The preconditions of this command grouped together.
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions { get; }
	/// <inheritdoc cref="IPriorityAttribute.Priority"/>
	int Priority { get; }

	/// <summary>
	/// Executes the command.
	/// </summary>
	/// <param name="context">
	/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
	/// </param>
	/// <param name="args">The arguments for this command.</param>
	/// <returns></returns>
	ValueTask<IResult> ExecuteAsync(IContext context, IReadOnlyList<object?> args);
}
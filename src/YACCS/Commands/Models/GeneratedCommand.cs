using System.Diagnostics;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models;

/// <summary>
/// A generated command.
/// </summary>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public abstract class GeneratedCommand : IImmutableCommand
{
	private readonly int _PriorityDifference;

	/// <inheritdoc />
	public virtual IReadOnlyList<object> Attributes => Source.Attributes;
	/// <inheritdoc />
	public virtual Type ContextType => Source.ContextType;
	/// <inheritdoc />
	public virtual bool IsHidden => Source.IsHidden;
	/// <inheritdoc />
	public virtual int MaxLength => Source.MaxLength;
	/// <inheritdoc />
	public virtual int MinLength => Source.MinLength;
	/// <inheritdoc />
	public virtual IReadOnlyList<IImmutableParameter> Parameters => Source.Parameters;
	/// <inheritdoc />
	public virtual IReadOnlyList<IReadOnlyList<string>> Paths => Source.Paths;
	/// <inheritdoc />
	public virtual IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions => Source.Preconditions;
	/// <inheritdoc />
	public virtual string PrimaryId => Source.PrimaryId;
	/// <inheritdoc />
	public virtual int Priority => Source.Priority - _PriorityDifference;
	/// <inheritdoc />
	public IImmutableCommand Source { get; }
	IEnumerable<object> IQueryableEntity.Attributes => Attributes;
	IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
	IEnumerable<IReadOnlyList<string>> IQueryableCommand.Paths => Paths;
	private string DebuggerDisplay => this.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates a new <see cref="GeneratedCommand"/>.
	/// </summary>
	/// <param name="source">The source of this generated command.</param>
	/// <param name="priorityDifference">The amount to lower the priority by.</param>
	protected GeneratedCommand(IImmutableCommand source, int priorityDifference)
	{
		Source = source;
		_PriorityDifference = priorityDifference;
	}

	/// <inheritdoc />
	public abstract ValueTask<IResult> ExecuteAsync(
		IContext context,
		IReadOnlyList<object?> args);

	/// <inheritdoc />
	public virtual bool IsValidContext(Type type)
		=> Source.IsValidContext(type);
}
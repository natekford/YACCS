using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models;

/// <summary>
/// A generated command.
/// </summary>
/// <param name="source">The source of this generated command.</param>
/// <param name="priorityDifference">
/// <inheritdoc cref="PriorityDifference" path="/summary"/>
/// </param>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public abstract class GeneratedCommand(IImmutableCommand source, int priorityDifference)
	: IImmutableCommand
{
	/// <inheritdoc />
	public virtual IReadOnlyList<object> Attributes => Source.Attributes;
	/// <inheritdoc />
	public virtual IReadOnlyCollection<string> Categories => Source.Categories;
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
	public virtual int Priority => Source.Priority - PriorityDifference;
	/// <summary>
	/// The amount to lower the priority by in relation to the source's priority.
	/// </summary>
	public virtual int PriorityDifference { get; set; } = priorityDifference;
	/// <inheritdoc />
	public IImmutableCommand Source { get; } = source;
	IEnumerable<object> IQueryableEntity.Attributes => Attributes;
	IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
	private string DebuggerDisplay => this.FormatForDebuggerDisplay();

	/// <inheritdoc />
	public abstract ValueTask<IResult> ExecuteAsync(
		IContext context,
		IReadOnlyList<object?> args);

	/// <inheritdoc />
	public virtual bool IsValidContext(Type type)
		=> Source.IsValidContext(type);
}
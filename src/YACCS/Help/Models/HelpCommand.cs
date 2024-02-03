using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models;

/// <summary>
/// Information to display about a command in a help command.
/// </summary>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class HelpCommand : HelpItem<IImmutableCommand>
{
	/// <summary>
	/// The context type of this command.
	/// </summary>
	public HelpItem<Type> ContextType { get; }
	/// <summary>
	/// The parameters of this command.
	/// </summary>
	public IReadOnlyList<HelpParameter> Parameters { get; }
	/// <summary>
	/// The precondition groups of this command.
	/// </summary>
	public IReadOnlyDictionary<string, ILookup<Op, HelpItem<IPrecondition>>> Preconditions { get; }
	private string DebuggerDisplay => Item.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates a new <see cref="HelpCommand"/>.
	/// </summary>
	/// <param name="item">The command to present information about.</param>
	public HelpCommand(IImmutableCommand item)
		: base(item, item.Attributes, x => x is not IPrecondition)
	{
		ContextType = Create(item.ContextType);

		var parameters = ImmutableArray.CreateBuilder<HelpParameter>(item.Parameters.Count);
		foreach (var parameter in item.Parameters)
		{
			parameters.Add(new HelpParameter(parameter));
		}
		Parameters = parameters.MoveToImmutable();

		Preconditions = item.Preconditions.ToImmutablePreconditions();
	}
}
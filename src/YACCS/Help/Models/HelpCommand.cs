using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models;

/// <summary>
/// Information to display about a command in a help command.
/// </summary>
/// <remarks>
/// Creates a new <see cref="HelpCommand"/>.
/// </remarks>
/// <param name="item">The command to present information about.</param>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class HelpCommand(IImmutableCommand item)
	: HelpItem<IImmutableCommand>(item, item.Attributes, x => x is not IPrecondition)
{
	/// <summary>
	/// The context type of this command.
	/// </summary>
	public virtual HelpItem<Type> ContextType { get; }
		= item.ContextType.ToHelpItem();
	/// <summary>
	/// The parameters of this command.
	/// </summary>
	public virtual IReadOnlyList<HelpParameter> Parameters { get; }
		= item.Parameters.Select(x => new HelpParameter(x)).ToImmutableArray();
	/// <summary>
	/// The precondition groups of this command.
	/// </summary>
	public virtual IReadOnlyDictionary<string, ILookup<Op, HelpItem<IPrecondition>>> Preconditions { get; }
		= item.Preconditions.ToHelpItems();
	private string DebuggerDisplay => Item.FormatForDebuggerDisplay();
}
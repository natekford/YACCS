using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models;

/// <inheritdoc cref="IHelpCommand"/>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class HelpCommand : HelpItem<IImmutableCommand>, IHelpCommand
{
	/// <inheritdoc />
	public IHelpItem<Type> ContextType { get; }
	/// <inheritdoc />
	public IReadOnlyList<IHelpParameter> Parameters { get; }
	/// <inheritdoc />
	public IReadOnlyDictionary<string, ILookup<BoolOp, IHelpItem<IPrecondition>>> Preconditions { get; }
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

		Preconditions = item.Preconditions.ToImmutableDictionary(
			x => x.Key,
			x => x.Value
				.Select(x => (IHelpItem<IPrecondition>)new HelpItem<IPrecondition>(x))
				.ToLookup(x => x.Item.Op)
		);
	}
}
using System.Diagnostics;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models;

/// <summary>
/// Information to display about a parameter in a help command.
/// </summary>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class HelpParameter : HelpItem<IImmutableParameter>
{
	/// <summary>
	/// Whether or not this parameter is a remainder.
	/// </summary>
	public bool IsRemainder => Item.Length is null;
	/// <summary>
	/// The type of this parameter.
	/// </summary>
	public HelpItem<Type> ParameterType { get; }
	/// <summary>
	/// The precondition groups of this parameter.
	/// </summary>
	public IReadOnlyDictionary<string, ILookup<Op, HelpItem<IParameterPrecondition>>> Preconditions { get; }
	/// <summary>
	/// The specified type reader of this parameter.
	/// </summary>
	public HelpItem<ITypeReader>? TypeReader { get; }
	private string DebuggerDisplay => Item.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates a new <see cref="HelpParameter"/>.
	/// </summary>
	/// <param name="item">The parameter to present information about.</param>
	public HelpParameter(IImmutableParameter item)
		: base(item, item.Attributes, x => x is not IParameterPrecondition)
	{
		ParameterType = Create(item.ParameterType);
		TypeReader = item.TypeReader is ITypeReader tr ? new HelpItem<ITypeReader>(tr) : null;

		Preconditions = item.Preconditions.ToImmutablePreconditions();
	}
}
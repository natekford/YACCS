using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models;

/// <inheritdoc cref="IHelpParameter"/>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class HelpParameter : HelpItem<IImmutableParameter>, IHelpParameter
{
	/// <inheritdoc />
	public bool IsRemainder => Item.Length is null;
	/// <inheritdoc />
	public IHelpItem<Type> ParameterType { get; }
	/// <inheritdoc />
	public IReadOnlyDictionary<string, ILookup<BoolOp, IHelpItem<IParameterPrecondition>>> Preconditions { get; }
	/// <inheritdoc />
	public IHelpItem<ITypeReader>? TypeReader { get; }
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

		Preconditions = item.Preconditions.ToImmutableDictionary(
			x => x.Key,
			x => x.Value
				.Select(x => (IHelpItem<IParameterPrecondition>)new HelpItem<IParameterPrecondition>(x))
				.ToLookup(x => x.Item.Op)
		);
	}
}

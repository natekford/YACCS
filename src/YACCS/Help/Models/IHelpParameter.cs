
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models;

/// <summary>
/// Information to display about a parameter in a help command.
/// </summary>
public interface IHelpParameter : IHelpItem<IImmutableParameter>
{
	/// <summary>
	/// Whether or not this parameter is a remainder.
	/// </summary>
	bool IsRemainder { get; }
	/// <summary>
	/// The type of this parameter.
	/// </summary>
	IHelpItem<Type> ParameterType { get; }
	/// <summary>
	/// The precondition groups of this parameter.
	/// </summary>
	IReadOnlyDictionary<string, ILookup<BoolOp, IHelpItem<IParameterPrecondition>>> Preconditions { get; }
	/// <summary>
	/// The specified type reader of this parameter.
	/// </summary>
	IHelpItem<ITypeReader>? TypeReader { get; }
}

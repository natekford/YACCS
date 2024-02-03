using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models;

/// <summary>
/// An immutable parameter.
/// </summary>
public interface IImmutableParameter : IImmutableEntity, IQueryableParameter
{
	/// <inheritdoc cref="IMutableParameter.DefaultValue"/>
	object? DefaultValue { get; }
	/// <inheritdoc cref="IMutableParameter.HasDefaultValue"/>
	bool HasDefaultValue { get; }
	/// <inheritdoc cref="ILengthAttribute.Length"/>
	int? Length { get; }
	/// <summary>
	/// The current parameter name.
	/// </summary>
	string ParameterName { get; }
	/// <summary>
	/// The preconditions of this parameter grouped together.
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyList<IParameterPrecondition>> Preconditions { get; }
	/// <inheritdoc cref="IMutableParameter.TypeReader"/>
	ITypeReader? TypeReader { get; }
}
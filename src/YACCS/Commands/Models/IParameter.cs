using YACCS.TypeReaders;

namespace YACCS.Commands.Models;

/// <summary>
/// A mutable parameter.
/// </summary>
public interface IParameter : IEntityBase, IQueryableParameter
{
	/// <summary>
	/// The default value of this parameter.
	/// </summary>
	object? DefaultValue { get; set; }
	/// <summary>
	/// Whether or not <see cref="DefaultValue"/> has actually been set.
	/// </summary>
	bool HasDefaultValue { get; set; }
	/// <summary>
	/// The type reader to use, if <see langword="null"/> one will be
	/// retrieved.
	/// </summary>
	ITypeReader? TypeReader { get; set; }

	/// <summary>
	/// Creates a new <see cref="IImmutableParameter"/>.
	/// </summary>
	/// <returns></returns>
	IImmutableParameter ToImmutable();
}

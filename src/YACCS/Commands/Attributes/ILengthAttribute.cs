using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes;

/// <summary>
/// An attribute to be used for setting <see cref="IImmutableParameter.Length"/>.
/// </summary>
public interface ILengthAttribute
{
	/// <summary>
	/// The expected amount of args when parsing this <see cref="IParameter"/>.
	/// A value of <see langword="null"/> means any amount of args is valid.
	/// </summary>
	int? Length { get; }
}
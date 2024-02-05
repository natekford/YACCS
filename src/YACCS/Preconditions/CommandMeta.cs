using YACCS.Commands.Models;

namespace YACCS.Preconditions;

/// <summary>
/// Contains a command and a parameter.
/// </summary>
/// <remarks>
/// Creates a new <see cref="CommandMeta"/>.
/// </remarks>
/// <param name="Command">
/// The command being validated.
/// </param>
/// <param name="Parameter">
/// The parameter being validated.
/// </param>
public readonly record struct CommandMeta(
	IImmutableCommand Command,
	IImmutableParameter Parameter
);
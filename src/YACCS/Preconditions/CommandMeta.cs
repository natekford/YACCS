using YACCS.Commands.Models;

namespace YACCS.Preconditions;

/// <summary>
/// Contains a command and a parameter.
/// </summary>
/// <remarks>
/// Creates a new <see cref="CommandMeta"/>.
/// </remarks>
/// <param name="command">
/// <inheritdoc cref="Command" path="/summary"/>
/// </param>
/// <param name="parameter">
/// <inheritdoc cref="Parameter" path="/summary"/>
/// </param>
public readonly struct CommandMeta(
	IImmutableCommand command,
	IImmutableParameter parameter)
{
	/// <summary>
	/// The command being validated.
	/// </summary>
	public IImmutableCommand Command { get; } = command;
	/// <summary>
	/// The parameter being validated.
	/// </summary>
	public IImmutableParameter Parameter { get; } = parameter;
}
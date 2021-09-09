using YACCS.Commands.Models;

namespace YACCS.Preconditions
{
	/// <summary>
	/// Contains a command and a parameter.
	/// </summary>
	public readonly struct CommandMeta
	{
		/// <summary>
		/// The command being validated.
		/// </summary>
		public IImmutableCommand Command { get; }
		/// <summary>
		/// The parameter being validated.
		/// </summary>
		public IImmutableParameter Parameter { get; }

		/// <summary>
		/// Creates a new <see cref="CommandMeta"/>.
		/// </summary>
		/// <param name="command">
		/// <inheritdoc cref="Command" path="/summary"/>
		/// </param>
		/// <param name="parameter">
		/// <inheritdoc cref="Parameter" path="/summary"/>
		/// </param>
		public CommandMeta(IImmutableCommand command, IImmutableParameter parameter)
		{
			Command = command;
			Parameter = parameter;
		}
	}
}
using YACCS.Commands.Models;

namespace YACCS.Preconditions
{
	public readonly struct CommandMeta
	{
		public IImmutableCommand Command { get; }
		public IImmutableParameter Parameter { get; }

		public CommandMeta(IImmutableCommand command, IImmutableParameter parameter)
		{
			Command = command;
			Parameter = parameter;
		}
	}
}
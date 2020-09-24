using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public readonly struct CommandInfo
	{
		public IImmutableCommand Command { get; }
		public IImmutableParameter? Parameter { get; }

		public CommandInfo(IImmutableCommand command, IImmutableParameter? parameter)
		{
			Command = command;
			Parameter = parameter;
		}
	}
}
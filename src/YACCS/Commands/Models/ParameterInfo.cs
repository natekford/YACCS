namespace YACCS.Commands.Models
{
	public readonly struct ParameterInfo
	{
		public IImmutableCommand Command { get; }
		public IImmutableParameter Parameter { get; }

		public ParameterInfo(IImmutableCommand command, IImmutableParameter parameter)
		{
			Command = command;
			Parameter = parameter;
		}
	}
}
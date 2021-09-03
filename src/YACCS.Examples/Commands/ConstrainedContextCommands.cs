using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Examples.Commands
{
	[ContextMustImplement(typeof(IMessagable))]
	public class ConstrainedContextCommands : CommandGroup<IContext>
	{
		public IMessagable Messagable { get; protected set; } = null!;

		public override Task BeforeExecutionAsync(IImmutableCommand command, IContext context)
		{
			Messagable = (IMessagable)context;
			return base.BeforeExecutionAsync(command, context);
		}

		[Command(nameof(Message))]
		public Task Message([Remainder] string message)
			=> Messagable.SendMessageAsync(message);
	}
}
using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;

namespace YACCS.Sample.Commands;

public class ConstrainedContextCommands : CommandGroup<IContext>
{
	[InjectContext]
	public required IMessagable Messagable { get; set; }

	[Command(nameof(Message))]
	public Task Message([Remainder] string message)
		=> Messagable.SendMessageAsync(message);
}
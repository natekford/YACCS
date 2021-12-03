using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;

namespace YACCS.Examples.Commands;

public class ConstrainedContextCommands : CommandGroup<IContext>
{
	[InjectContext]
	public IMessagable Messagable { get; set; } = null!;

	[Command(nameof(Message))]
	public Task Message([Remainder] string message)
		=> Messagable.SendMessageAsync(message);
}

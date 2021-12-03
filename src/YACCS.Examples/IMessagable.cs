namespace YACCS.Examples;

public interface IMessagable
{
	Task SendMessageAsync(string message);
}

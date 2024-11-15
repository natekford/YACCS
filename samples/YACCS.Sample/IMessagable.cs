namespace YACCS.Sample;

public interface IMessagable
{
	Task SendMessageAsync(string message);
}
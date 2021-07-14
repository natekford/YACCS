namespace YACCS.Commands
{
	public interface IContext<out T> : IContext
	{
		new T Source { get; }
	}
}
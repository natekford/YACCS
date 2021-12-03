namespace YACCS.Commands;

/// <inheritdoc />
public interface IContext<out T> : IContext
{
	/// <inheritdoc cref="IContext.Source"/>
	new T Source { get; }
}

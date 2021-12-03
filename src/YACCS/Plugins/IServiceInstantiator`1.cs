namespace YACCS.CommandAssemblies;

/// <inheritdoc />
public interface IServiceInstantiator<T> : IServiceInstantiator
{
	/// <inheritdoc cref="IServiceInstantiator.AddServicesAsync(object)"/>
	Task AddServicesAsync(T services);
}
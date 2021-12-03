namespace YACCS.CommandAssemblies;

/// <summary>
/// The base class for adding and configuring services in a plugin assembly.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ServiceInstantiator<T> : IServiceInstantiator<T>
{
	/// <inheritdoc />
	public abstract Task AddServicesAsync(T services);

	/// <inheritdoc />
	public abstract Task ConfigureServicesAsync(IServiceProvider services);

	Task IServiceInstantiator.AddServicesAsync(object services)
		=> AddServicesAsync((T)services);
}
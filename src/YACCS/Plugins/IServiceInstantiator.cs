namespace YACCS.CommandAssemblies;

/// <summary>
/// Defines methods for adding and configuring services.
/// </summary>
public interface IServiceInstantiator
{
	/// <summary>
	/// Adds services to <paramref name="services"/>.
	/// </summary>
	/// <param name="services">The object to add services to.</param>
	/// <returns></returns>
	Task AddServicesAsync(object services);

	/// <summary>
	/// Configures all the services which have been added.
	/// </summary>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <returns></returns>
	Task ConfigureServicesAsync(IServiceProvider services);
}
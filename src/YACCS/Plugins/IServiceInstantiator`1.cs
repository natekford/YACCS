namespace YACCS.CommandAssemblies;

/// <summary>
/// Defines methods for adding and configuring services.
/// </summary>
/// <typeparam name="TServiceCollection">The collection to add services to.</typeparam>
public interface IServiceInstantiator<TServiceCollection>
{
	/// <summary>
	/// Adds services to <paramref name="services"/>.
	/// </summary>
	/// <param name="services">The object to add services to.</param>
	/// <returns></returns>
	Task AddServicesAsync(TServiceCollection services);

	/// <summary>
	/// Configures all the services which have been added.
	/// </summary>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <returns></returns>
	Task ConfigureServicesAsync(IServiceProvider services);
}
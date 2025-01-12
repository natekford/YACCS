using System;
using System.Threading.Tasks;

namespace YACCS.Plugins;

/// <summary>
/// Defines methods for adding and configuring services.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class PluginAttribute : Attribute
{
	/// <summary>
	/// The languages this assembly supports.
	/// </summary>
	public virtual string[] SupportedCultures { get; init; } = [];

	/// <summary>
	/// Adds services to <paramref name="services"/>.
	/// </summary>
	/// <param name="services">
	/// The object to add services to. This will usually be IServiceCollection.
	/// </param>
	/// <returns></returns>
	public virtual Task AddServicesAsync(object services)
		=> Task.CompletedTask;

	/// <summary>
	/// Configures all the services which have been added.
	/// </summary>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <returns></returns>
	public virtual Task ConfigureServicesAsync(IServiceProvider services)
		=> Task.CompletedTask;
}
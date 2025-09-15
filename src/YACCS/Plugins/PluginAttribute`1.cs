using System;
using System.Threading.Tasks;

namespace YACCS.Plugins;

/// <inheritdoc />
/// <typeparam name="TServiceCollection"></typeparam>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public abstract class PluginAttribute<TServiceCollection> : PluginAttribute
{
	/// <inheritdoc />
	public override Task AddServicesAsync(object services)
	{
		if (services is TServiceCollection tServices)
		{
		}
		throw new InvalidOperationException("Invalid service collection type provided to a plugin instantiator.");
	}

	/// <inheritdoc cref="AddServicesAsync(object)" />
	public abstract Task AddServicesAsync(TServiceCollection services);
}
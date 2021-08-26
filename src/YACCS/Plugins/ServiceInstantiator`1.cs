using System;
using System.Threading.Tasks;

namespace YACCS.CommandAssemblies
{
	public abstract class ServiceInstantiator<T> : IServiceInstantiator<T>
	{
		public abstract Task AddServicesAsync(T services);

		public abstract Task ConfigureServicesAsync(IServiceProvider services);

		Task IServiceInstantiator.AddServicesAsync(object services)
			=> AddServicesAsync((T)services);
	}
}
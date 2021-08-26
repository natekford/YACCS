using System;
using System.Threading.Tasks;

namespace YACCS.CommandAssemblies
{
	public interface IServiceInstantiator
	{
		Task AddServicesAsync(object services);

		Task ConfigureServicesAsync(IServiceProvider services);
	}
}
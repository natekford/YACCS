using System.Threading.Tasks;

namespace YACCS.CommandAssemblies
{
	public interface IServiceInstantiator<T> : IServiceInstantiator
	{
		Task AddServicesAsync(T services);
	}
}
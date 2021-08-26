using System;

namespace YACCS.CommandAssemblies
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class ServiceInstantiatorAttribute : Attribute
	{
		public IServiceInstantiator Instantiator { get; }

		public ServiceInstantiatorAttribute(Type instantiatorType)
		{
			Instantiator = instantiatorType.CreateInstance<IServiceInstantiator>();
		}
	}
}
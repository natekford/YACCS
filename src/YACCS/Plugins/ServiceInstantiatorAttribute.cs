using System;

namespace YACCS.CommandAssemblies
{
	/// <summary>
	/// Specifies what type to use for adding and configuring services.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class ServiceInstantiatorAttribute : Attribute
	{
		/// <summary>
		/// The service instantiator used to add and configure services.
		/// </summary>
		public IServiceInstantiator Instantiator { get; }

		/// <summary>
		/// Creates a new <see cref="ServiceInstantiatorAttribute"/>.
		/// </summary>
		/// <param name="instantiatorType">The type to create an instance of.</param>
		public ServiceInstantiatorAttribute(Type instantiatorType)
		{
			Instantiator = instantiatorType.CreateInstance<IServiceInstantiator>();
		}
	}
}
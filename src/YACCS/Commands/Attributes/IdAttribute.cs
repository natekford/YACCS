using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class IdAttribute : Attribute, IIdAttribute
	{
		public string Id { get; }

		public IdAttribute(string id)
		{
			Id = id;
		}
	}
}
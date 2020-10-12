using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class IdAttribute : Attribute, IIdAttribute
	{
		public string Id { get; }

		public IdAttribute(string id)
		{
			Id = id;
		}
	}
}
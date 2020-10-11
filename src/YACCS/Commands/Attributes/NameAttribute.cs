using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class NameAttribute : Attribute, INameAttribute
	{
		public string Name { get; }

		public NameAttribute(string name)
		{
			Name = name;
		}
	}
}
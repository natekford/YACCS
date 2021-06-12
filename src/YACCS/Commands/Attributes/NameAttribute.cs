using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class NameAttribute : Attribute, INameAttribute
	{
		public virtual string Name { get; }

		public NameAttribute(string name)
		{
			Name = name;
		}
	}
}
using System;

using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GeneratedNamedArgumentsAttribute : Attribute
	{
		public IImmutableCommand? Source { get; }

		public GeneratedNamedArgumentsAttribute()
		{
		}

		public GeneratedNamedArgumentsAttribute(IImmutableCommand source)
		{
			Source = source;
		}
	}
}
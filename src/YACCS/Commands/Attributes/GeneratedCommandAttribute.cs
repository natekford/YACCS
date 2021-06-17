using System;

using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GeneratedCommandAttribute : Attribute
	{
		public IImmutableCommand Source { get; }

		public GeneratedCommandAttribute(IImmutableCommand source)
		{
			Source = source;
		}
	}
}
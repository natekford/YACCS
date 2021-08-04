using System;

namespace YACCS.TypeReaders
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class GetServiceMethodAttribute : Attribute
	{
	}
}
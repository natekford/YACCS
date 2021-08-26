using System;

namespace YACCS.CommandAssemblies
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class PluginAttribute : Attribute
	{
	}
}
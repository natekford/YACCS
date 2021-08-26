using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace YACCS.CommandAssemblies
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class SupportedCulturesAttribute : Attribute
	{
		public ImmutableArray<CultureInfo> SupportedCultures { get; }

		public SupportedCulturesAttribute(params string[] supportedCultures)
		{
			SupportedCultures = supportedCultures.Select(CultureInfo.GetCultureInfo).ToImmutableArray();
		}
	}
}
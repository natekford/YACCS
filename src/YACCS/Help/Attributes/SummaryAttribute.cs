using System;

namespace YACCS.Help.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class SummaryAttribute : Attribute, ISummaryAttribute
	{
		public string Summary { get; }

		public SummaryAttribute(string summary)
		{
			Summary = summary;
		}
	}
}
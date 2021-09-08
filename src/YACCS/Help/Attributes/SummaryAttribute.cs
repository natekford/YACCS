using System;

namespace YACCS.Help.Attributes
{
	/// <inheritdoc cref="ISummaryAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class SummaryAttribute : Attribute, ISummaryAttribute
	{
		/// <inheritdoc />
		public virtual string Summary { get; }

		/// <summary>
		/// Creates a new <see cref="Summary"/> and sets <see cref="Summary"/>
		/// to <paramref name="summary"/>.
		/// </summary>
		/// <param name="summary"></param>
		public SummaryAttribute(string summary)
		{
			Summary = summary;
		}
	}
}
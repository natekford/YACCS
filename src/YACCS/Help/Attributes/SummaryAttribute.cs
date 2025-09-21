using System;

namespace YACCS.Help.Attributes;

/// <inheritdoc cref="ISummarizableAttribute" />
/// <param name="summary">The summary of an item.</param>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public class SummaryAttribute(string summary)
	: Attribute, ISummaryAttribute
{
	/// <inheritdoc />
	public virtual string Summary { get; } = summary;
}
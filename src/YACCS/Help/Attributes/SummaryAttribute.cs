namespace YACCS.Help.Attributes;

/// <inheritdoc cref="ISummaryAttribute" />
/// <summary>
/// Creates a new <see cref="Summary"/>.
/// </summary>
/// <param name="summary">
/// <inheritdoc cref="Summary" path="/summary"/>
/// </param>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class SummaryAttribute(string summary)
	: Attribute, ISummaryAttribute
{
	/// <inheritdoc />
	public virtual string Summary { get; } = summary;
}
namespace YACCS.Help.Attributes;

/// <inheritdoc cref="ISummaryAttribute" />
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class SummaryAttribute : Attribute, ISummaryAttribute
{
	/// <inheritdoc />
	public virtual string Summary { get; }

	/// <summary>
	/// Creates a new <see cref="Summary"/>.
	/// </summary>
	/// <param name="summary">
	/// <inheritdoc cref="Summary" path="/summary"/>
	/// </param>
	public SummaryAttribute(string summary)
	{
		Summary = summary;
	}
}

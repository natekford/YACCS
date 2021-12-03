using YACCS.Help.Models;

namespace YACCS.Help.Attributes;

/// <summary>
/// An attribute for setting <see cref="IHelpItem{T}.Summary"/>.
/// </summary>
public interface ISummaryAttribute
{
	/// <summary>
	/// The summary of an item.
	/// </summary>
	string Summary { get; }
}

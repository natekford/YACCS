using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.Help.Attributes;

/// <summary>
/// An attribute which can show a summary about itself.
/// </summary>
public interface ISummarizableAttribute
{
	/// <summary>
	/// Formats this attribute for being displayed to the end user.
	/// </summary>
	/// <param name="context">
	/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
	/// </param>
	/// <param name="formatProvider">
	/// The provider to format any <see cref="string.Format(IFormatProvider, string, object)"/>
	/// with.
	/// </param>
	/// <returns>A <see cref="string"/> representing this attribute.</returns>
	ValueTask<string> GetSummaryAsync(IContext context, IFormatProvider? formatProvider = null);
}
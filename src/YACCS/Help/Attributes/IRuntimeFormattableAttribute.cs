using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.Help.Attributes
{
	/// <summary>
	/// An attribute which can be formatted at runtime instead of
	/// using <see cref="object.ToString"/>.
	/// </summary>
	public interface IRuntimeFormattableAttribute
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
		ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null);
	}
}
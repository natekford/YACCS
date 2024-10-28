using System;
using System.Threading.Tasks;

using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;

namespace YACCS.Commands.Attributes;

/// <inheritdoc cref="IPriorityAttribute"/>
/// <summary>
/// Creates a new <see cref="PriorityAttribute"/>.
/// </summary>
/// <param name="priority">
/// <inheritdoc cref="Priority" path="/summary"/>
/// </param>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public class PriorityAttribute(int priority)
	: Attribute, IPriorityAttribute, IRuntimeFormattableAttribute
{
	/// <inheritdoc />
	public int Priority { get; } = priority;

	/// <inheritdoc />
	public virtual ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new(formatProvider.Format($"{Keys.Priority:key} {Priority:value}"));
}
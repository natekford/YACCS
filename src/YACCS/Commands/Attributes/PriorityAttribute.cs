using System;
using System.Threading.Tasks;

using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;

namespace YACCS.Commands.Attributes
{
	/// <inheritdoc cref="IPriorityAttribute"/>
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class PriorityAttribute : Attribute, IPriorityAttribute, IRuntimeFormattableAttribute
	{
		/// <inheritdoc />
		public int Priority { get; }

		/// <summary>
		/// Creates a new <see cref="PriorityAttribute"/> and sets <see cref="Priority"/>
		/// to <paramref name="priority"/>.
		/// </summary>
		/// <param name="priority"></param>
		public PriorityAttribute(int priority)
		{
			Priority = priority;
		}

		/// <inheritdoc />
		public virtual ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
			=> new(formatProvider.Format($"{Keys.Priority:key} {Priority:value}"));
	}
}
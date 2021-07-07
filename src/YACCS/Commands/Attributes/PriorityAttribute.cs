using System;
using System.Threading.Tasks;

using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class PriorityAttribute : Attribute, IPriorityAttribute, IRuntimeFormattableAttribute
	{
		public int Priority { get; }

		public PriorityAttribute(int priority)
		{
			Priority = priority;
		}

		public virtual ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
			=> new(formatProvider.Format($"{Keys.PRIORITY:k} {Priority:v}"));
	}
}
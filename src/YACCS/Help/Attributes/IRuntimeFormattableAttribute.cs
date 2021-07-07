using System;

using YACCS.Commands;

namespace YACCS.Help.Attributes
{
	public interface IRuntimeFormattableAttribute
	{
		string Format(IContext context, IFormatProvider? formatProvider = null);
	}
}
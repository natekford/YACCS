using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.Help.Attributes
{
	public interface IRuntimeFormattableAttribute
	{
		ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null);
	}
}
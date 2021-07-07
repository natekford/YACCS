using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.Help.Attributes
{
	public interface IAsyncRuntimeFormattableAttribute
	{
		ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null);
	}
}
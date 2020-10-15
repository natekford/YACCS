using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.Help.Attributes
{
	public interface IAsyncRuntimeFormattableAttribute
	{
		ValueTask<IReadOnlyList<TaggedString>> FormatAsync(IContext context);
	}
}
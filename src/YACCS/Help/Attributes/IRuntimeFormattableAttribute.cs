using System.Collections.Generic;

using YACCS.Commands;

namespace YACCS.Help.Attributes
{
	public interface IRuntimeFormattableAttribute
	{
		IReadOnlyList<TaggedString> Format(IContext context);
	}
}
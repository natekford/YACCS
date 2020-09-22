using System.Collections.Generic;

namespace YACCS.Commands.Attributes
{
	public interface ICommandAttribute
	{
		bool AllowInheritance { get; }
		IReadOnlyList<string> Names { get; }
	}
}
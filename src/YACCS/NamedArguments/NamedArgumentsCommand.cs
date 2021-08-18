using System.Collections.Generic;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments
{
	public sealed class NamedArgumentsCommand : NamedArgumentsCommand<Dictionary<string, object?>>
	{
		public NamedArgumentsCommand(IImmutableCommand source) : base(source)
		{
		}
	}
}
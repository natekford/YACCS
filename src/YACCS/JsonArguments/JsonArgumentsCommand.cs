using System;
using System.Diagnostics;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.JsonArguments
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class JsonArgumentsCommand : GeneratedCommand
	{
		public JsonArgumentsCommand(IImmutableCommand source) : base(source)
		{
		}

		public override ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
		{
			throw new NotImplementedException();
		}
	}
}
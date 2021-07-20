﻿using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples.Preconditions
{
	public class DisabledAttribute : PreconditionAttribute
	{
		public override ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context)
			=> new(new FailureResult("Command is disabled."));
	}
}
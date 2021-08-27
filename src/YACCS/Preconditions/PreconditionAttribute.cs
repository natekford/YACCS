using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public abstract class PreconditionAttribute : GroupablePreconditionAttribute, IPrecondition
	{
		public virtual Task AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception)
			=> Task.CompletedTask;

		public virtual Task BeforeExecutionAsync(IImmutableCommand command, IContext context)
			=> Task.CompletedTask;

		public abstract ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context);
	}
}
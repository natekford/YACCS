using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class PreconditionAttribute : GroupablePreconditionAttribute, IPrecondition
	{
		public virtual ValueTask AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception)
			=> new();

		public virtual ValueTask BeforeExecutionAsync(IImmutableCommand command, IContext context)
			=> new();

		public abstract ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context);
	}
}
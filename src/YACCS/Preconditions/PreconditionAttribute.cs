using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class PreconditionAttribute : Attribute, IPrecondition
	{
		public virtual Task AfterExecutionAsync(IImmutableCommand command, IContext context)
			=> Task.CompletedTask;

		public abstract Task<IResult> CheckAsync(IImmutableCommand command, IContext context);
	}
}

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <summary>
	/// The base class for a precondition attribute.
	/// </summary>
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public abstract class PreconditionAttribute
		: GroupablePreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public virtual Task AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception)
			=> Task.CompletedTask;

		/// <inheritdoc />
		public virtual Task BeforeExecutionAsync(IImmutableCommand command, IContext context)
			=> Task.CompletedTask;

		/// <inheritdoc />
		public abstract ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context);
	}
}
using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public abstract class ParameterPreconditionAttribute
		: GroupablePreconditionAttribute, IParameterPrecondition
	{
		Task<IResult> IParameterPrecondition.CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value)
			=> CheckAsync(command, parameter, context, value);

		protected abstract Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value);
	}
}
using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public abstract class ParameterPreconditionAttribute
		: GroupablePreconditionAttribute, IParameterPrecondition
	{
		ValueTask<IResult> IParameterPrecondition.CheckAsync(
			CommandMeta meta,
			IContext context,
			object? value)
			=> CheckAsync(meta, context, value);

		protected abstract ValueTask<IResult> CheckAsync(
			CommandMeta meta,
			IContext context,
			object? value);
	}
}
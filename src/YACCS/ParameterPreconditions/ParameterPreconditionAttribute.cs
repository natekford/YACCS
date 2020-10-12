using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public abstract class ParameterPreconditionAttribute : Attribute, IParameterPrecondition
	{
		public abstract Task<IResult> CheckAsync(ParameterInfo parameter, IContext context, object? value);
	}
}
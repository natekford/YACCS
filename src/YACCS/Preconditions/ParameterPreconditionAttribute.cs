
using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <summary>
	/// The base class for a parameter precondition attribute.
	/// </summary>
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public abstract class ParameterPreconditionAttribute
		: GroupablePreconditionAttribute, IParameterPrecondition
	{
		/// <inheritdoc />
		public abstract ValueTask<IResult> CheckAsync(
			CommandMeta meta,
			IContext context,
			object? value);
	}
}
using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions;

/// <inheritdoc />
public interface IParameterPrecondition<in TValue> : IParameterPrecondition
{
	/// <inheritdoc cref="IParameterPrecondition.CheckAsync(CommandMeta, IContext, object?)"/>
	ValueTask<IResult> CheckAsync(CommandMeta meta, IContext context, TValue? value);
}
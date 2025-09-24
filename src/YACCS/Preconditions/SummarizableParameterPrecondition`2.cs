using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Help.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions;

/// <summary>
/// The base class for a parameter precondition that can be summarized.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TValue"></typeparam>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public abstract class SummarizableParameterPrecondition<TContext, TValue>
	: ParameterPrecondition<TContext, TValue>, ISummarizableAttribute
	where TContext : IContext
{
	/// <inheritdoc cref="ISummarizableAttribute.GetSummaryAsync(IContext, IFormatProvider?)" />
	public abstract ValueTask<string> GetSummaryAsync(TContext context, IFormatProvider? formatProvider = null);

	ValueTask<string> ISummarizableAttribute.GetSummaryAsync(IContext context, IFormatProvider? formatProvider)
	{
		if (context is TContext tContext)
		{
			return GetSummaryAsync(tContext, formatProvider);
		}
		return new(Result.InvalidContext.Response);
	}
}
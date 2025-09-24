using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Help.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions;

/// <summary>
/// The base class for a precondition k
/// </summary>
/// <typeparam name="TContext"></typeparam>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public abstract class SummarizablePrecondition<TContext>
	: Precondition<TContext>, ISummarizableAttribute
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
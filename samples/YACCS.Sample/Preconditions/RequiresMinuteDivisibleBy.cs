using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Sample.Preconditions;

[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
public class RequiresMinuteDivisibleBy(int divisor)
	: Precondition<IContext>, IRuntimeFormattableAttribute
{
	public int Divisor { get; } = divisor;
	public virtual string FallbackErrorMessage { get; set; } = "Current minute must be divisible by {0}.";

	public override ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context)
	{
		if (DateTime.Now.Minute % Divisor != 0)
		{
			return new(Result.Failure(GetErrorMessage()));
		}
		return new(Result.EmptySuccess);
	}

	public ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new(GetErrorMessage());

	private string GetErrorMessage()
	{
		var format = Localize.This(nameof(RequiresMinuteDivisibleBy), FallbackErrorMessage);
		return string.Format(format, Divisor);
	}
}
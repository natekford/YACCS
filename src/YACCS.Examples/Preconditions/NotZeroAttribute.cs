using YACCS.Commands;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples.Preconditions
{
	public class NotZero : ParameterPrecondition<IContext, int>, IRuntimeFormattableAttribute
	{
		public virtual string FallbackErrorMessage { get; set; } = "Cannot be zero.";

		public override ValueTask<IResult> CheckAsync(
			CommandMeta meta,
			IContext context,
			int value)
		{
			if (value == 0)
			{
				return new(new FailureResult(GetErrorMessage()));
			}
			return new(SuccessResult.Instance);
		}

		public ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
			=> new(GetErrorMessage());

		private string GetErrorMessage()
			=> Localize.This(nameof(NotZero), FallbackErrorMessage);
	}
}
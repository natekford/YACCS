using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples
{
	public class NotZero : ParameterPreconditionAttribute, IRuntimeFormattableAttribute
	{
		public virtual string FallbackErrorMessage { get; set; } = "Cannot be zero.";

		public ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
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

		protected override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value)
			=> this.CheckAsync<IContext, int>(command, parameter, context, value, CheckAsync);

		private string GetErrorMessage()
			=> Localize.This("NotZero", FallbackErrorMessage);
	}
}
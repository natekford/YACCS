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
		public static Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			int value)
		{
			if (value == 0)
			{
				return new FailureResult(GetFailureMessage()).AsTask();
			}
			return SuccessResult.Instance.Task;
		}

		public ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
			=> new(GetFailureMessage());

		protected override Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value)
			=> this.CheckAsync<IContext, int>(command, parameter, context, value, CheckAsync);

		private static string GetFailureMessage()
			=> Localize.This("NotZero", "Cannot be zero.");
	}
}
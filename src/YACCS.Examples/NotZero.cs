using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples
{
	public class NotZero :
		ParameterPreconditionAttribute,
		IRuntimeFormattableAttribute,
		IUsesLocalizer
	{
		private const string DEFAULT = "Cannot be zero.";

		public virtual ILocalizer? Localizer { get; set; }
		protected virtual string FailureMessage => Localizer?.Get("NotZero") ?? DEFAULT;

		public Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			int value)
		{
			if (value == 0)
			{
				return new FailureResult(FailureMessage).AsTask();
			}
			return SuccessResult.Instance.Task;
		}

		public IReadOnlyList<TaggedString> Format(IContext context)
		{
			return new TaggedString[]
			{
				new(Tag.String, FailureMessage),
			};
		}

		protected override Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value)
			=> this.CheckAsync<IContext, int>(command, parameter, context, value, CheckAsync);
	}
}
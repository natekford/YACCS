using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples
{
	public class NotZero : ParameterPreconditionAttribute, IRuntimeFormattableAttribute
	{
		private const string _Message = "Cannot be zero.";
		private static readonly TaggedString[] _Tags = new[] { new TaggedString(Tag.String, _Message) };
		private static readonly Task<IResult> _Task = new FailureResult(_Message).AsTask();

		public static Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			int value)
		{
			if (value == 0)
			{
				return _Task;
			}
			return SuccessResult.Instance.Task;
		}

		public IReadOnlyList<TaggedString> Format(IContext context)
			=> _Tags;

		protected override Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value)
			=> this.CheckAsync<IContext, int>(command, parameter, context, value, CheckAsync);
	}
}
using System.Diagnostics.CodeAnalysis;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public class InteractiveResult<TValue> : IInteractiveResult<TValue>
	{
		public IResult InnerResult { get; }
		[MaybeNull]
		public TValue Value { get; }

		public InteractiveResult(TValue value)
		{
			Value = value;
			InnerResult = SuccessResult.Instance;
		}

		public InteractiveResult(IResult result)
		{
			InnerResult = result;
		}
	}
}
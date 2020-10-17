using System.Diagnostics.CodeAnalysis;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public class InteractivityResult<TValue> : IInteractivityResult<TValue>
	{
		public IResult InnerResult { get; }
		[MaybeNull]
		public TValue Value { get; }

		public InteractivityResult(TValue value)
		{
			Value = value;
			InnerResult = SuccessResult.Instance.Sync;
		}

		public InteractivityResult(IResult result)
		{
			InnerResult = result;
		}
	}
}
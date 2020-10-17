using System.Diagnostics.CodeAnalysis;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public interface IInteractivityResult<out TValue> : INestedResult
	{
		[MaybeNull]
		TValue Value { get; }
	}
}
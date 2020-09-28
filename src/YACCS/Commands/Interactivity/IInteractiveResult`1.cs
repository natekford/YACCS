using System.Diagnostics.CodeAnalysis;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public interface IInteractiveResult<out TValue> : INestedResult
	{
		[MaybeNull]
		TValue Value { get; }
	}
}
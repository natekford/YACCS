using System.Diagnostics.CodeAnalysis;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public interface IInteractiveResult<TValue> : INestedResult
	{
		[MaybeNull]
		TValue Value { get; }
	}
}
using YACCS.Commands.Models;

namespace YACCS.Results
{
	public interface ICommandResult : INestedResult
	{
		IImmutableCommand? Command { get; }
		IImmutableParameter? Parameter { get; }
	}
}
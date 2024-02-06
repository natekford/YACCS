using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands;

/// <summary>
/// A result returned from
/// <see cref="CommandServiceBase.ExecuteAsync(IContext, ReadOnlySpan{char})"/>.
/// </summary>
/// <remarks>
/// For the default implementation of <see cref="CommandServiceBase"/>,
/// these results will only be errors or <see cref="Results.CachedResults.Success"/>.
/// All command results will get sent to
/// <see cref="CommandServiceBase.CommandExecutedAsync(CommandExecutedEventArgs)"/>.
/// <br/>
/// <br/>
/// For the default implementation of <see cref="CommandService"/>, the above method
/// fires <see cref="CommandService.CommandExecuted"/>.
/// </remarks>
public interface IExecuteResult : INestedResult
{
	/// <summary>
	/// The command attempting to be executed.
	/// </summary>
	/// <remarks>
	/// This will be <see langword="null"/> if there are string splitting errors
	/// or a command is simply not found.
	/// </remarks>
	IImmutableCommand? Command { get; }
	/// <summary>
	/// The parameter which had a precondition or type reader fail.
	/// </summary>
	/// <remarks>
	/// This will be <see langword="null"/> if there are no parameter errors.
	/// Otherwise, if a parameter has a parsing error or does not pass each of its
	/// parameter precondition groups, this property will be set.
	/// </remarks>
	IImmutableParameter? Parameter { get; }
}
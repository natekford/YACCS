using System;
using System.Threading.Tasks;

namespace YACCS.Commands;

/// <summary>
/// Processes commands.
/// </summary>
public interface ICommandQueue
{
	/// <summary>
	/// Adds a command to the queue so it can be executed.
	/// </summary>
	/// <param name="command">The command to add to the queue.</param>
	ValueTask EnqueueAsync(Func<Task> command);
}
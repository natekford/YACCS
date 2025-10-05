using MorseCode.ITask;

using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.TypeReaders;

/// <summary>
/// The base class for a <see cref="ITypeReader"/> dealing with commands.
/// </summary>
public abstract class CommandsTypeReader : TypeReader<IReadOnlyList<IImmutableCommand>>
{
	/// <inheritdoc />
	public override ITask<ITypeReaderResult<IReadOnlyList<IImmutableCommand>>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		if (input.Length == 0)
		{
			return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.ParseFailed.Task;
		}

		var commands = GetCommands(context.Services);
		var found = GetMatchingCommands(context, commands, input).Where(IsValidCommand);
		if (!found.Any())
		{
			return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.ParseFailed.Task;
		}
		return Success([.. found]).AsITask();
	}

	/// <summary>
	/// Filters for commands that match <paramref name="input"/>.
	/// </summary>
	/// <param name="context">The context executing this command.</param>
	/// <param name="commands">All registered commands.</param>
	/// <param name="input">The input to match.</param>
	/// <returns>An unsorted enumerable of matching commands.</returns>
	protected abstract IEnumerable<IImmutableCommand> GetMatchingCommands(
		IContext context,
		ICommandService commands,
		ReadOnlyMemory<string> input);

	/// <summary>
	/// Determines if a command should be returned.
	/// </summary>
	/// <param name="command">The command to check.</param>
	/// <returns>A bool indicating if the command should be returned.</returns>
	protected virtual bool IsValidCommand(IImmutableCommand command)
	{
		// Generated items have a source and that source gives them the same
		// names/properties, so they should be ignored since they are copies
		return command.Source is null
			// Hidden commands should be ignored too
			&& !command.GetAttributes<HiddenAttribute>().Any();
	}

	[GetServiceMethod]
	private static ICommandService GetCommands(IServiceProvider services)
		=> services.GetRequiredService<ICommandService>();
}
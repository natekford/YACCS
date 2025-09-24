using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help.Models;

namespace YACCS.Help;

/// <summary>
/// Converts a command to a readable
/// </summary>
/// <param name="typeNames">
/// <inheritdoc cref="TypeNames" path="/summary"/>
/// </param>
/// <param name="formatProvider">
/// <inheritdoc cref="FormatProvider" path="/summary"/>
/// </param>
public class StringHelpFactory(
	IReadOnlyDictionary<Type, string> typeNames,
	IFormatProvider? formatProvider = null
) : IHelpFactory<IContext, string>
{
	/// <summary>
	/// The commands in this program.
	/// </summary>
	protected ConcurrentDictionary<IImmutableCommand, HelpCommand> Commands = [];
	/// <summary>
	/// The format provider to use when formatting strings.
	/// </summary>
	protected IFormatProvider? FormatProvider { get; } = formatProvider;
	/// <summary>
	/// Type names to use for displaying types.
	/// </summary>
	protected IReadOnlyDictionary<Type, string> TypeNames { get; } = typeNames;

	/// <inheritdoc />
	public async ValueTask<string> CreateHelpAsync(IContext context, IImmutableCommand command)
	{
		var help = GetHelpCommand(command);
		var builder = GetBuilder(context);

		builder.AddNames(help.Item.Paths);
		builder.AddSummary(help.Summary);
		await builder.AddAttributesAsync(help.Attributes).ConfigureAwait(false);
		await builder.AddPreconditionsAsync(help.Preconditions).ConfigureAwait(false);
		await builder.AddParametersAsync(help.Parameters).ConfigureAwait(false);
		return builder.ToString();
	}

	/// <summary>
	/// Creates a new <see cref="StringHelpBuilder"/>.
	/// </summary>
	/// <param name="context">The context invoking this help command.</param>
	/// <returns>A new help builder.</returns>
	protected virtual StringHelpBuilder GetBuilder(IContext context)
		=> new(context, TypeNames, FormatProvider);

	/// <summary>
	/// Creates a new <see cref="HelpCommand"/>.
	/// </summary>
	/// <param name="command">The command to create a help command for.</param>
	/// <returns>A new help command.</returns>
	protected virtual HelpCommand GetHelpCommand(IImmutableCommand command)
	{
		while (command.Source is not null)
		{
			command = command.Source;
		}
		return Commands.GetOrAdd(command, k => new(k));
	}
}
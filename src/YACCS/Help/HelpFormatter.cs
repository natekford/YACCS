using System.Collections.Concurrent;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help.Models;

namespace YACCS.Help;

/// <summary>
/// Formats a command to a string.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="HelpFormatter"/>.
/// </remarks>
/// <param name="typeNames">
/// <inheritdoc cref="TypeNames" path="/summary"/>
/// </param>
/// <param name="formatProvider">
/// <inheritdoc cref="FormatProvider" path="/summary"/>
/// </param>
public class HelpFormatter(
	IReadOnlyDictionary<Type, string> typeNames,
	IFormatProvider? formatProvider = null)
	: IHelpFormatter
{
	private readonly ConcurrentDictionary<IImmutableCommand, HelpCommand> _Commands = [];
	/// <summary>
	/// The format provider to use when formatting strings.
	/// </summary>
	protected IFormatProvider? FormatProvider { get; } = formatProvider;
	/// <summary>
	/// Type names to use for displaying types.
	/// </summary>
	protected IReadOnlyDictionary<Type, string> TypeNames { get; } = typeNames;

	/// <inheritdoc />
	public async ValueTask<string> FormatAsync(IContext context, IImmutableCommand command)
	{
		var help = GetHelpCommand(command);
		var builder = GetBuilder(context);

		builder.AppendNames(help.Item.Paths);
		builder.AppendSummary(help.Summary);
		await builder.AppendAttributesAsync(help.Attributes).ConfigureAwait(false);
		await builder.AppendPreconditionsAsync(help.Preconditions).ConfigureAwait(false);
		await builder.AppendParametersAsync(help.Parameters).ConfigureAwait(false);
		return builder.ToString();
	}

	/// <summary>
	/// Creates a new <see cref="HelpBuilder"/>.
	/// </summary>
	/// <param name="context">The context invoking this help command.</param>
	/// <returns>A new help builder.</returns>
	protected virtual HelpBuilder GetBuilder(IContext context)
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
		return _Commands.GetOrAdd(command, k => new(k));
	}
}
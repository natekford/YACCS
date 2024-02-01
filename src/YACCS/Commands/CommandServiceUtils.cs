using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands;

/// <summary>
/// Utilities for command services.
/// </summary>
public static class CommandServiceUtils
{
	/// <summary>
	/// The default quote character.
	/// </summary>
	public const char QUOTE = '"';
	/// <summary>
	/// The default space character.
	/// </summary>
	public const char SPACE = ' ';
	internal const string DEBUGGER_DISPLAY = "{DebuggerDisplay,nq}";
	/// <summary>
	/// A set containing only <see cref="QUOTE"/>.
	/// </summary>
	public static ImmutableHashSet<char> Quotes { get; } = [QUOTE];

	/// <summary>
	/// Checks that every precondition group <paramref name="command"/> has is valid with
	/// <paramref name="context"/>.
	/// </summary>
	/// <param name="command">The command to check the preconditions of.</param>
	/// <param name="context">The context is invoking <paramref name="command"/>.</param>
	/// <returns>A result indicating success or failure.</returns>
	public static ValueTask<IResult> CanExecuteAsync(
		this IImmutableCommand command,
		IContext context)
	{
		return command.Preconditions.ProcessAsync(static (precondition, state) =>
		{
			var (command, context) = state;
			return precondition.CheckAsync(command, context);
		}, (command, context));
	}

	/// <summary>
	/// Checks that every precondition group <paramref name="parameter"/> has is valid
	/// with <paramref name="context"/> and <paramref name="value"/>.
	/// </summary>
	/// <param name="command">The command <paramref name="parameter"/> belongs to.</param>
	/// <param name="parameter">The parameter to check the preconditions of.</param>
	/// <param name="context">The context which is invoking <paramref name="context"/>.</param>
	/// <param name="value">The value to check preconditions with.</param>
	/// <returns>A result indicating success or failure.</returns>
	public static ValueTask<IResult> CanExecuteAsync(
		this IImmutableCommand command,
		IImmutableParameter parameter,
		IContext context,
		object? value)
	{
		var meta = new CommandMeta(command, parameter);
		return parameter.Preconditions.ProcessAsync(static (precondition, state) =>
		{
			var (meta, context, value) = state;
			return precondition.CheckAsync(meta, context, value);
		}, (meta, context, value));
	}

	/// <summary>
	/// Gets all exceptions that are in <paramref name="e"/>.
	/// </summary>
	/// <param name="e">The args to get exceptions from.</param>
	/// <returns>An enumerable of all the exceptions.</returns>
	public static IEnumerable<Exception> GetAllExceptions(this CommandExecutedEventArgs e)
	{
		var enumerable = Enumerable.Empty<Exception>();
		if (e.BeforeExceptions is not null)
		{
			enumerable = enumerable.Concat(e.BeforeExceptions);
		}
		if (e.DuringException is not null)
		{
			enumerable = enumerable.Append(e.DuringException);
		}
		if (e.AfterExceptions is not null)
		{
			enumerable = enumerable.Concat(e.AfterExceptions);
		}
		return enumerable;
	}

	/// <summary>
	/// Determines if <paramref name="input"/> starts with <paramref name="prefix"/>
	/// and then returns a substring with <paramref name="prefix"/> removed.
	/// </summary>
	/// <param name="input">The input to check if it has a prefix.</param>
	/// <param name="prefix">The prefix to look for.</param>
	/// <param name="output">
	/// <paramref name="input"/> with <paramref name="prefix"/> removed from the start.
	/// </param>
	/// <param name="comparisonType">The comparison type for string equality.</param>
	/// <returns>A bool indicating success or failure.</returns>
	public static bool HasPrefix(
		this ReadOnlySpan<char> input,
		ReadOnlySpan<char> prefix,
		[NotNullWhen(true)] out ReadOnlySpan<char> output,
		StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
	{
		var success = input.StartsWith(prefix, comparisonType);
		output = success ? input[prefix.Length..] : default;
		return success;
	}

	internal static string FormatForDebuggerDisplay(this IQueryableCommand item)
	{
		var name = item.Paths?.FirstOrDefault()?.ToString() ?? "NULL";
		return $"Name = {name}, Parameter Count = {item.Parameters.Count}";
	}

	internal static string FormatForDebuggerDisplay(this IQueryableParameter item)
		=> $"Name = {item.OriginalParameterName}, Type = {item.ParameterType}";

	internal static string FormatForDebuggerDisplay(this IResult item)
		=> $"IsSuccess = {item.IsSuccess}, Response = {item.Response}";

	private static ValueTask<IResult> ProcessAsync<TPrecondition, TState>(
		this IReadOnlyDictionary<string, IReadOnlyList<TPrecondition>> preconditions,
		Func<TPrecondition, TState, ValueTask<IResult>> converter,
		TState state)
		where TPrecondition : IGroupablePrecondition
	{
		if (preconditions.Count == 0)
		{
			return new(Success.Instance);
		}

		static async ValueTask<IResult> PrivateProcessAsync(
			IReadOnlyDictionary<string, IReadOnlyList<TPrecondition>> preconditions,
			Func<TPrecondition, TState, ValueTask<IResult>> converter,
			TState state)
		{
			// Preconditions are grouped but cannot be subgrouped
			// So treat logic as group is surrounded by parantheses but inside isn't
			// Each group must succeed for a command to be valid
			foreach (var group in preconditions)
			{
				var orResult = default(IResult?);
				var andResult = default(IResult?);
				foreach (var precondition in group.Value)
				{
					// An AND has already failed, no need to check other ANDs
					if (precondition.Op == Op.And && andResult?.IsSuccess == false)
					{
						continue;
					}

					var result = await converter(precondition, state).ConfigureAwait(false);
					// OR: Any success = instant success, go to next group
					if (precondition.Op == Op.Or)
					{
						orResult = result;
						// Do NOT return directly from here, each group must succeed
						if (result.IsSuccess)
						{
							break;
						}
					}
					// AND: Any failure = skip other ANDs, only check further ORs
					else if (precondition.Op == Op.And)
					{
						andResult = result;
					}
				}

				// Both result are null? Success because not failure
				// Either result is success? Success because a result is success
				if (orResult?.IsSuccess != false && andResult?.IsSuccess != false)
				{
					continue;
				}

				// Group failed, command is a failure
				return (orResult ?? andResult)!;
			}
			return Success.Instance;
		}

		return PrivateProcessAsync(preconditions, converter, state);
	}
}
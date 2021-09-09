using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands
{
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
		public static IImmutableSet<char> Quotes { get; } = new[] { QUOTE }.ToImmutableHashSet();

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
			return command.Preconditions.ProcessAsync((precondition, state) =>
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
			return parameter.Preconditions.ProcessAsync((precondition, state) =>
			{
				var (meta, context, value) = state;
				return precondition.CheckAsync(meta, context, value);
			}, (meta, context, value));
		}

		/// <summary>
		/// Returns all distinct items directly inside <paramref name="node"/> and recursively
		/// inside all of its edges until every edge that has <paramref name="node"/> as an
		/// ancestor has been iterated through.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="node">The node to get items from.</param>
		/// <param name="predicate">The predicate to check nodes with.</param>
		/// <returns>
		/// A set of all distinct nodes from <paramref name="node"/> and its edges.
		/// </returns>
		public static HashSet<TValue> GetAllDistinctItems<TKey, TValue>(
			this INode<TKey, TValue> node,
			Func<TValue, bool>? predicate = null)
		{
			static IEnumerable<TValue> GetItems(INode<TKey, TValue> node)
			{
				foreach (var item in node.Items)
				{
					yield return item;
				}
				foreach (var edge in node.Edges)
				{
					foreach (var item in GetItems(edge))
					{
						yield return item;
					}
				}
			}

			predicate ??= _ => true;

			var set = new HashSet<TValue>();
			foreach (var item in GetItems(node))
			{
				if (predicate(item))
				{
					set.Add(item);
				}
			}
			return set;
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
			this string input,
			string prefix,
			[NotNullWhen(true)] out string? output,
			StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			if (input.Length <= prefix.Length || !input.StartsWith(prefix, comparisonType))
			{
				output = null;
				return false;
			}

			output = input[prefix.Length..];
			return true;
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
				return new(SuccessResult.Instance);
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
					IResult groupResult = SuccessResult.Instance;
					foreach (var precondition in group.Value)
					{
						// An AND has already failed, no need to check other ANDs
						if (precondition.Op == BoolOp.And && !groupResult.IsSuccess)
						{
							continue;
						}

						var result = await converter(precondition, state).ConfigureAwait(false);
						// OR: Any success = instant success, go to next group
						if (precondition.Op == BoolOp.Or && result.IsSuccess)
						{
							// Do NOT return directly from here, each group must succeed
							groupResult = SuccessResult.Instance;
							break;
						}
						// AND: Any failure = skip other ANDs, only check further ORs
						else if (precondition.Op == BoolOp.And && !result.IsSuccess)
						{
							groupResult = result;
						}
					}
					// Any group failed, command is a failure
					if (!groupResult.IsSuccess)
					{
						return groupResult;
					}
				}
				return SuccessResult.Instance;
			}

			return PrivateProcessAsync(preconditions, converter, state);
		}
	}
}
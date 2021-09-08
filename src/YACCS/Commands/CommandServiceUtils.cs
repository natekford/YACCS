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
		public const char QUOTE = '"';
		public const char SPACE = ' ';
		internal const string DEBUGGER_DISPLAY = "{DebuggerDisplay,nq}";
		public static IImmutableSet<char> Quotes { get; } = new[] { QUOTE }.ToImmutableHashSet();

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

		public static ValueTask<IResult> CanExecuteAsync(
			this IImmutableParameter parameter,
			CommandMeta meta,
			IContext context,
			object? value)
		{
			return parameter.Preconditions.ProcessAsync((precondition, state) =>
			{
				var (meta, context, value) = state;
				return precondition.CheckAsync(meta, context, value);
			}, (meta, context, value));
		}

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

		public static ValueTask<IResult> ProcessAsync<TPrecondition, TState>(
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

		internal static string FormatForDebuggerDisplay(this IQueryableCommand item)
		{
			var name = item.Paths?.FirstOrDefault()?.ToString() ?? "NULL";
			return $"Name = {name}, Parameter Count = {item.Parameters.Count}";
		}

		internal static string FormatForDebuggerDisplay(this IQueryableParameter item)
			=> $"Name = {item.OriginalParameterName}, Type = {item.ParameterType}";

		internal static string FormatForDebuggerDisplay(this IResult item)
			=> $"IsSuccess = {item.IsSuccess}, Response = {item.Response}";
	}
}
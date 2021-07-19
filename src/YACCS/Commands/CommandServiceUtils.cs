using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands
{
	public static class CommandServiceUtils
	{
		public const char QUOTE = '"';
		public const char SPACE = ' ';
		public static readonly IImmutableSet<char> Quotes = new[] { QUOTE }.ToImmutableHashSet();
		internal const string DEBUGGER_DISPLAY = "{DebuggerDisplay,nq}";

		public static void AddRange(
			this CommandServiceBase commandService,
			IEnumerable<IImmutableCommand> enumerable)
		{
			foreach (var command in enumerable)
			{
				commandService.Commands.Add(command);
			}
		}

		public static async Task AddRangeAsync(
			this CommandServiceBase commandService,
			IAsyncEnumerable<IImmutableCommand> enumerable)
		{
			await foreach (var command in enumerable)
			{
				commandService.Commands.Add(command);
			}
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this Type type,
			IServiceProvider services)
		{
			await foreach (var command in type.GetDirectCommandsAsync(services))
			{
				yield return command;
			}
			foreach (var nested in type.GetNestedTypes())
			{
				await foreach (var command in nested.GetAllCommandsAsync(services))
				{
					yield return command;
				}
			}
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this IEnumerable<Assembly> assemblies,
			IServiceProvider services)
		{
			foreach (var assembly in assemblies)
			{
				await foreach (var command in assembly.GetAllCommandsAsync(services))
				{
					yield return command;
				}
			}
		}

		public static IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this Assembly assembly,
			IServiceProvider services)
			=> assembly.GetExportedTypes().GetDirectCommandsAsync(services);

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

		public static IReadOnlyList<TValue> GetAllItems<TKey, TValue>(
			this INode<TKey, TValue> node,
			Predicate<TValue>? predicate = null)
		{
			static int GetMaximumSize(INode<TKey, TValue> node)
			{
				var size = node.Items.Count;
				foreach (var edge in node.Edges)
				{
					size += GetMaximumSize(edge);
				}
				return size;
			}

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

			var set = new HashSet<TValue>(GetMaximumSize(node));
			foreach (var item in GetItems(node))
			{
				if (predicate(item))
				{
					set.Add(item);
				}
			}

			return set.ToImmutableArray();
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetDirectCommandsAsync(
			this IEnumerable<Type> types,
			IServiceProvider services)
		{
			foreach (var type in types)
			{
				await foreach (var command in type.GetDirectCommandsAsync(services))
				{
					yield return command;
				}
			}
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetDirectCommandsAsync(
			this Type type,
			IServiceProvider services)
		{
			if (type.IsAbstract)
			{
				yield break;
			}

			var commands = type.CreateMutableCommands();
			if (commands.Count == 0)
			{
				yield break;
			}

			var group = type.CreateInstance<ICommandGroup>();
			await group.OnCommandBuildingAsync(services, commands).ConfigureAwait(false);

			// Commands have been modified by whoever implemented them
			// We can now return them in an immutable state
			foreach (var command in commands)
			{
				await foreach (var immutable in command.ToMultipleImmutableAsync(services))
				{
					yield return immutable;
				}
			}
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

		public static ValueTask<IResult> ProcessAsync<T>(
			this IReadOnlyDictionary<string, IReadOnlyList<T>> preconditions,
			Func<T, ValueTask<IResult>> converter)
			where T : IGroupablePrecondition
		{
			if (preconditions.Count == 0)
			{
				return new(SuccessResult.Instance);
			}

			static async ValueTask<IResult> PrivateProcessAsync(
				IReadOnlyDictionary<string, IReadOnlyList<T>> preconditions,
				Func<T, ValueTask<IResult>> converter)
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

						var result = await converter(precondition).ConfigureAwait(false);
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

			return PrivateProcessAsync(preconditions, converter);
		}

		internal static List<ICommand> CreateMutableCommands(this Type type)
		{
			const BindingFlags FLAGS = 0
				| BindingFlags.Public
				| BindingFlags.Instance
				| BindingFlags.FlattenHierarchy;

			var commands = new List<ICommand>();
			foreach (var method in type.GetMethods(FLAGS))
			{
				var command = method
					.GetCustomAttributes()
					.OfType<ICommandAttribute>()
					.SingleOrDefault();
				if (command is null || (!command.AllowInheritance && type != method.DeclaringType))
				{
					continue;
				}

				commands.Add(new ReflectionCommand(method));
			}

			return commands;
		}

		internal static string FormatForDebuggerDisplay(this IQueryableCommand item)
		{
			var name = item.Names?.FirstOrDefault()?.ToString() ?? "NULL";
			return $"Name = {name}, Parameter Count = {item.Parameters.Count}";
		}

		internal static string FormatForDebuggerDisplay(this IQueryableParameter item)
			=> $"Name = {item.OriginalParameterName}, Type = {item.ParameterType}";
	}
}
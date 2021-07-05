using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public static class CommandServiceUtils
	{
		public const char QUOTE = '"';
		public const char SPACE = ' ';
		public static readonly IImmutableSet<char> Quotes = new[] { QUOTE }.ToImmutableHashSet();
		internal const string DEBUGGER_DISPLAY = "{DebuggerDisplay,nq}";

		public static void AddRange(
			this CommandService commandService,
			IEnumerable<IImmutableCommand> enumerable)
		{
			foreach (var command in enumerable)
			{
				commandService.Commands.Add(command);
			}
		}

		public static async Task AddRangeAsync(
			this CommandService commandService,
			IAsyncEnumerable<IImmutableCommand> enumerable)
		{
			await foreach (var command in enumerable)
			{
				commandService.Commands.Add(command);
			}
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this Type type)
		{
			var commands = await type.GetDirectCommandsAsync().ConfigureAwait(false);
			foreach (var command in commands)
			{
				yield return command;
			}
			foreach (var nested in type.GetNestedTypes())
			{
				await foreach (var command in nested.GetAllCommandsAsync())
				{
					yield return command;
				}
			}
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this IEnumerable<Assembly> assemblies)
		{
			foreach (var assembly in assemblies)
			{
				await foreach (var command in assembly.GetAllCommandsAsync())
				{
					yield return command;
				}
			}
		}

		public static IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this Assembly assembly)
			=> assembly.GetExportedTypes().GetDirectCommandsAsync();

		public static IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync<T>()
			where T : ICommandGroup, new()
			=> typeof(T).GetAllCommandsAsync();

		public static IEnumerable<Exception> GetAllExceptions(this CommandExecutedEventArgs e)
		{
			var enumerable = Enumerable.Empty<Exception>();
			if (e.BeforeExceptions != null)
			{
				enumerable = enumerable.Concat(e.BeforeExceptions);
			}
			if (e.DuringException != null)
			{
				enumerable = enumerable.Append(e.DuringException);
			}
			if (e.AfterExceptions != null)
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
			this IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				var commands = await type.GetDirectCommandsAsync().ConfigureAwait(false);
				foreach (var command in commands)
				{
					yield return command;
				}
			}
		}

		public static Task<IEnumerable<IImmutableCommand>> GetDirectCommandsAsync(
			this Type type)
		{
			var commands = type.CreateMutableCommands();
			if (commands.Count == 0)
			{
				return Task.FromResult<IEnumerable<IImmutableCommand>>(Array.Empty<IImmutableCommand>());
			}

			static async Task<IEnumerable<IImmutableCommand>> GetDirectCommandsAsync(
				Type type,
				List<ICommand> commands)
			{
				var group = type.CreateInstance<ICommandGroup>();
				await group.OnCommandBuildingAsync(commands).ConfigureAwait(false);

				// Commands have been modified by whoever implemented them
				// We can now return them in an immutable state
				return commands.SelectMany(x =>
				{
					try
					{
						return x.MakeMultipleImmutable();
					}
					catch (Exception e)
					{
						throw new InvalidOperationException(
							$"An exception occurred while building the command '{x.Names?.FirstOrDefault()}'.", e);
					}
				});
			}
			return GetDirectCommandsAsync(type, commands);
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
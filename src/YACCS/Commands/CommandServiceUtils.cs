using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public static class CommandServiceUtils
	{
		public const char InternallyUsedQuote = '"';
		public const char InternallyUsedSeparator = ' ';
		public static readonly IImmutableSet<char> InternallyUsedQuotes
			= new[] { InternallyUsedQuote }.ToImmutableHashSet();

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

		public static Task<IReadOnlyList<IImmutableCommand>> GetDirectCommandsAsync(
			this Type type)
		{
			var commands = type.CreateMutableCommands();
			if (commands.Count == 0)
			{
				return Task.FromResult<IReadOnlyList<IImmutableCommand>>(Array.Empty<IImmutableCommand>());
			}

			static async Task<IReadOnlyList<IImmutableCommand>> GetDirectCommandsAsync(
				Type type,
				List<ICommand> commands)
			{
				var group = type.CreateInstance<ICommandGroup>();
				await group.OnCommandBuildingAsync(commands).ConfigureAwait(false);

				// Commands have been modified by whoever implemented them
				// We can now return them in an immutable state
				return commands.SelectMany(x => x.ToImmutable()).ToArray();
			}
			return GetDirectCommandsAsync(type, commands);
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

		internal static async Task<ITypeReaderResult> ProcessArrayTypeReadersAsync(
			this PreconditionCache cache,
			IImmutableParameter parameter,
			ITypeReader reader,
			IReadOnlyList<string> input,
			int startIndex,
			int length)
		{
			// If an array, test each value one by one
			var values = new object?[length];
			for (var i = startIndex; i < startIndex + length; ++i)
			{
				var result = await cache.GetResultAsync(reader, input[i]).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess)
				{
					return result;
				}
				values[i - startIndex] = result.Value;
			}

			// Copy the values from the type reader result list to an array of the parameter type
			var output = Array.CreateInstance(parameter.ElementType, values.Length);
			for (var i = 0; i < values.Length; ++i)
			{
				output.SetValue(values[i], i);
			}
			return TypeReaderResult<object>.FromSuccess(output);
		}
	}
}
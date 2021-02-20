using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

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
			this Type type,
			ICommandServiceConfig? config = null)
		{
			var commands = await type.GetDirectCommandsAsync(config).ConfigureAwait(false);
			foreach (var command in commands)
			{
				yield return command;
			}
			foreach (var nested in type.GetNestedTypes())
			{
				await foreach (var command in nested.GetAllCommandsAsync(config))
				{
					yield return command;
				}
			}
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this IEnumerable<Assembly> assemblies,
			ICommandServiceConfig? config = null)
		{
			foreach (var assembly in assemblies)
			{
				await foreach (var command in assembly.GetAllCommandsAsync(config))
				{
					yield return command;
				}
			}
		}

		public static IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this Assembly assembly,
			ICommandServiceConfig? config = null)
			=> assembly.GetExportedTypes().GetDirectCommandsAsync(config);

		public static IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync<T>(
			ICommandServiceConfig? config = null)
			where T : ICommandGroup, new()
			=> typeof(T).GetAllCommandsAsync(config);

		public static async IAsyncEnumerable<IImmutableCommand> GetDirectCommandsAsync(
			this IEnumerable<Type> types,
			ICommandServiceConfig? config = null)
		{
			foreach (var type in types)
			{
				var commands = await type.GetDirectCommandsAsync(config).ConfigureAwait(false);
				foreach (var command in commands)
				{
					yield return command;
				}
			}
		}

		public static Task<IEnumerable<IImmutableCommand>> GetDirectCommandsAsync(
			this Type type,
			ICommandServiceConfig? config = null)
		{
			var commands = type.CreateMutableCommands(config);
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
				return commands.SelectMany(x => x.ToImmutable());
			}
			return GetDirectCommandsAsync(type, commands);
		}

		internal static List<ICommand> CreateMutableCommands(
			this Type type,
			ICommandServiceConfig? config = null)
		{
			const BindingFlags FLAGS = 0
				| BindingFlags.Public
				| BindingFlags.Instance
				| BindingFlags.FlattenHierarchy;
			config ??= CommandServiceConfig.Default;

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

				foreach (var name in command.Names)
				{
					if (name.Contains(config.Separator))
					{
						throw new ArgumentException("Command names cannot contain the separator.", name);
					}
				}

				commands.Add(new ReflectionCommand(method));
			}

			return commands;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public static class CommandServiceUtils
	{
		public static T CreateInstance<T>(Type type)
		{
			object instance;
			try
			{
				instance = Activator.CreateInstance(type);
			}
			catch (Exception ex)
			{
				throw new ArgumentException(
					$"Unable to create an instance of {type.Name}. Is it missing a public parameterless constructor?", nameof(type), ex);
			}
			if (instance is T t)
			{
				return t;
			}
			throw new ArgumentException(
				$"{type.Name} does not implement {typeof(T).FullName}.", nameof(type));
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this Type type)
		{
			var commands = await type.GetDirectCommandsAsync().ConfigureAwait(false);
			foreach (var command in commands)
			{
				yield return command;
			}
			foreach (var t in type.GetNestedTypes())
			{
				await foreach (var command in GetAllCommandsAsync(t))
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

		public static async IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync(
			this Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				await foreach (var command in type.GetAllCommandsAsync())
				{
					yield return command;
				}
			}
		}

		public static IAsyncEnumerable<IImmutableCommand> GetAllCommandsAsync<T>()
			where T : ICommandGroup, new()
			=> typeof(T).GetAllCommandsAsync();

		public static async Task<IReadOnlyList<IImmutableCommand>> GetDirectCommandsAsync(
			this Type type)
		{
			var commands = type.CreateMutableCommands();
			if (commands.Count > 0)
			{
				var group = CreateInstance<ICommandGroup>(type);
				await group.OnCommandBuildingAsync(commands).ConfigureAwait(false);

				// Commands have been modified by whoever implemented them
				// We can now return them in an immutable state
				return commands.Select(x => x.ToCommand()).ToArray();
			}
			return Array.Empty<IImmutableCommand>();
		}

		public static IReadOnlyList<ICommand> GetDirectCommandsMutable(this Type type)
					=> type.CreateMutableCommands();

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
	}

	public static class TypeUtils
	{
		public static bool IsGenericOf(this Type type, Type definition)
			=> type.IsGenericType && type.GetGenericTypeDefinition() == definition;
	}

	internal static class IServiceProviderUtils
	{
		public static T GetRequiredService<T>(this IServiceProvider provider)
		{
			var service = provider.GetService(typeof(T));
			if (service is T t)
			{
				return t;
			}
			throw new InvalidOperationException($"{typeof(T).Name} does not have a registered service.");
		}
	}
}
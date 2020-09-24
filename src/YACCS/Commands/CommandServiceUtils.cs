using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public static class CommandServiceUtils
	{
		public static async Task Add(
			this ICommandService service,
			IAsyncEnumerable<IImmutableCommand> commands)
		{
			await foreach (var command in commands)
			{
				service.Add(command);
			}
		}

		public static async Task<IReadOnlyList<IImmutableCommand>> CreateCommandsAsync(
			this Type type)
		{
			const BindingFlags FLAGS = 0
				| BindingFlags.Public
				| BindingFlags.Instance
				| BindingFlags.FlattenHierarchy;

			List<ICommand>? list = null;
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

				list ??= new List<ICommand>();
				list.Add(new ReflectionCommand(type, method));
			}

			if (list != null)
			{
				object instance;
				try
				{
					instance = Activator.CreateInstance(type);
				}
				catch (Exception e)
				{
					throw new InvalidCommandTypeException(
						$"Unable to create an instance of {type.Name}. Is it missing a public parameterless constructor?", e);
				}
				if (!(instance is ICommandGroup group))
				{
					throw new InvalidCommandTypeException(
						$"{type.Name} does not implement {nameof(ICommandGroup)}.");
				}

				await group.OnCommandBuildingAsync(list).ConfigureAwait(false);

				// Commands have been modified by whoever implemented them
				// We can now return them in an immutable state
				return list.Select(x => x.ToCommand()).ToArray();
			}
			return Array.Empty<IImmutableCommand>();
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetCommandsAsync(
			this IEnumerable<Assembly> assemblies)
		{
			foreach (var assembly in assemblies)
			{
				await foreach (var command in assembly.GetCommandsAsync())
				{
					yield return command;
				}
			}
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetCommandsAsync(
			this Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				await foreach (var command in type.GetCommandsAsync())
				{
					yield return command;
				}
			}
		}

		public static async IAsyncEnumerable<IImmutableCommand> GetCommandsAsync(
			this Type type)
		{
			var commands = await type.CreateCommandsAsync().ConfigureAwait(false);
			foreach (var command in commands)
			{
				yield return command;
			}
			foreach (var t in type.GetNestedTypes())
			{
				await foreach (var command in GetCommandsAsync(t))
				{
					yield return command;
				}
			}
		}

		public static IEnumerable<TypeReaderInfo> GetTypeReaders(
			this IEnumerable<Assembly> assemblies)
		{
			foreach (var assembly in assemblies)
			{
				foreach (var typeReader in assembly.GetTypeReaders())
				{
					yield return typeReader;
				}
			}
		}

		public static IEnumerable<TypeReaderInfo> GetTypeReaders(
			this Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				var attr = type.GetCustomAttribute<TypeReaderTargetTypesAttribute>();
				if (attr == null)
				{
					continue;
				}

				object instance;
				try
				{
					instance = Activator.CreateInstance(type);
				}
				catch (Exception e)
				{
					throw new InvalidCommandTypeException(
						$"Unable to create an instance of {type.Name}. Is it missing a public parameterless constructor?", e);
				}
				if (!(instance is ITypeReader typeReader))
				{
					throw new InvalidCommandTypeException(
						$"{type.Name} does not implement {nameof(ITypeReader)}.");
				}
				yield return new TypeReaderInfo(attr.TargetTypes, typeReader);
			}
		}

		public static void Register(
			this ITypeReaderRegistry registry,
			IEnumerable<TypeReaderInfo> typeReaderInfos)
		{
			foreach (var typeReaderInfo in typeReaderInfos)
			{
				foreach (var type in typeReaderInfo.TargetTypes)
				{
					registry.Register(typeReaderInfo.Instance, type);
				}
			}
		}
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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public static class CommandCreationUtils
	{
		public static List<ReflectionCommand> CreateMutableCommands(this Type type)
		{
			const BindingFlags FLAGS = 0
				| BindingFlags.Public
				| BindingFlags.Instance
				| BindingFlags.FlattenHierarchy;

			var commands = new List<ReflectionCommand>();
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

		public static async IAsyncEnumerable<CreatedCommand> GetAllCommandsAsync(
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

		public static async IAsyncEnumerable<CreatedCommand> GetAllCommandsAsync(
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

		public static IAsyncEnumerable<CreatedCommand> GetAllCommandsAsync(
			this Assembly assembly,
			IServiceProvider services)
			=> assembly.GetExportedTypes().GetDirectCommandsAsync(services);

		public static async IAsyncEnumerable<CreatedCommand> GetDirectCommandsAsync(
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

		public static async IAsyncEnumerable<CreatedCommand> GetDirectCommandsAsync(
			this Type type,
			IServiceProvider services)
		{
			if (type.IsAbstract)
			{
				yield break;
			}

			var commands = type.CreateMutableCommands();
			foreach (var attr in type.GetCustomAttributes<OnCommandBuildingAttribute>())
			{
				await attr.ModifyCommands(services, commands).ConfigureAwait(false);
			}

			if (commands.Count == 0)
			{
				yield break;
			}

			var (properties, fields) = type.GetWritableMembers();
			var pConstraints = properties
				.Where(x => x.GetCustomAttribute<InjectContextAttribute>() is not null)
				.Select(x => x.PropertyType);
			var fConstraints = fields
				.Where(x => x.GetCustomAttribute<InjectContextAttribute>() is not null)
				.Select(x => x.FieldType);
			var constraints = pConstraints.Concat(fConstraints).Distinct().ToImmutableArray();
			foreach (var command in commands)
			{
				command.Attributes.Add(new ContextMustImplementAttribute(constraints));
			}

			// Commands have been modified by whoever implemented them
			// We can now return them in an immutable state
			foreach (var command in commands)
			{
				await foreach (var immutable in command.ToMultipleImmutableAsync(services))
				{
					yield return new(type, immutable);
				}
			}
		}

		public readonly struct CreatedCommand
		{
			public IImmutableCommand Command { get; }
			public Type DefiningType { get; }

			public CreatedCommand(Type definingType, IImmutableCommand command)
			{
				Command = command;
				DefiningType = definingType;
			}

			public void Deconstruct(out Type definingType, out IImmutableCommand command)
			{
				definingType = DefiningType;
				command = Command;
			}
		}
	}
}
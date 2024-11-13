using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;

namespace YACCS.Commands;

/// <summary>
/// Utilites for creating commands.
/// </summary>
public static class CommandCreationUtils
{
	/// <summary>
	/// Uses reflection to gather commands from <paramref name="type"/>.
	/// <br/>
	/// A command is any method which is:
	///		<see langword="public"/>,
	///		not <see langword="static"/>,
	///		and marked with <see cref="ICommandAttribute"/>.
	/// </summary>
	/// <param name="type">
	/// The type to gather commands from.
	/// This must implement <see cref="ICommandGroup{TContext}"/> if there are any
	/// commands defined inside it.
	/// </param>
	/// <returns>A list of <see cref="ReflectionCommand"/></returns>
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

	/// <summary>
	/// Calls <see cref="GetDirectCommandsAsync(Type, IServiceProvider)"/> for
	/// every <see cref="Type"/> defined within <paramref name="assembly"/>.
	/// </summary>
	/// <param name="assembly">The assembly to gather commands from.</param>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <returns>An async enumerable of <see cref="ImmutableReflectionCommand"/>.</returns>
	public static async IAsyncEnumerable<ImmutableReflectionCommand> GetAllCommandsAsync(
		this Assembly assembly,
		IServiceProvider services)
	{
		foreach (var type in assembly.GetExportedTypes())
		{
			await foreach (var command in type.GetAllCommandsAsync(services))
			{
				yield return command;
			}
		}
	}

	/// <summary>
	/// Calls <see cref="GetDirectCommandsAsync(Type, IServiceProvider)"/> for
	/// <paramref name="type"/> and all nested <see cref="Type"/>s within
	/// <paramref name="type"/>.
	/// </summary>
	/// <param name="type">The type to gather commands from.</param>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <returns>An async enumerable of <see cref="ImmutableReflectionCommand"/>.</returns>
	public static async IAsyncEnumerable<ImmutableReflectionCommand> GetAllCommandsAsync(
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

	/// <summary>
	/// Calls <see cref="CreateMutableCommands(Type)"/> and then modifies the
	/// resulting commands.
	/// </summary>
	/// <param name="type">
	/// The type to gather commands from.
	/// <br/>
	/// Abstract classes are ignored.
	/// </param>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <returns>An async enumerable of <see cref="ImmutableReflectionCommand"/>.</returns>
	public static async IAsyncEnumerable<ImmutableReflectionCommand> GetDirectCommandsAsync(
		this Type type,
		IServiceProvider services)
	{
		if (type.IsAbstract)
		{
			yield break;
		}

		var commands = type.CreateMutableCommands();
		if (typeof(IOnCommandBuilding).IsAssignableFrom(type))
		{
			var instance = type.CreateInstance<IOnCommandBuilding>();
			await instance.ModifyCommandsAsync(services, commands).ConfigureAwait(false);
		}

		if (commands.Count == 0)
		{
			yield break;
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

	/// <summary>
	/// Contains a newly created command and the <see cref="Type"/> which defined it.
	/// </summary>
	/// <param name="DefiningType">
	/// The newly created command.
	/// </param>
	/// <param name="Command">
	/// The <see cref="Type"/> that defined this command.
	/// </param>
	public readonly record struct ImmutableReflectionCommand(
		Type DefiningType,
		IImmutableCommand Command
	);
}
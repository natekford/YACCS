using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;

namespace YACCS.Commands
{
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
		/// <paramref name="type"/> and all nested <see cref="Type"/>s within
		/// <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type to gather commands from.</param>
		/// <param name="services">The services to use for dependency injection.</param>
		/// <returns>An async enumerable of <see cref="ReflectedCommand"/>.</returns>
		public static async IAsyncEnumerable<ReflectedCommand> GetAllCommandsAsync(
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
		/// Calls <see cref="GetDirectCommandsAsync(Type, IServiceProvider)"/> for
		/// every <see cref="Type"/> defined within each <see cref="Assembly"/>
		/// from <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="assemblies">The assemblies to gather commands from.</param>
		/// <param name="services">The services to use for dependency injection.</param>
		/// <returns>An async enumerable of <see cref="ReflectedCommand"/>.</returns>
		public static async IAsyncEnumerable<ReflectedCommand> GetAllCommandsAsync(
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

		/// <summary>
		/// Calls <see cref="GetDirectCommandsAsync(Type, IServiceProvider)"/> for
		/// every <see cref="Type"/> defined within <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly">The assembly to gather commands from.</param>
		/// <param name="services">The services to use for dependency injection.</param>
		/// <returns>An async enumerable of <see cref="ReflectedCommand"/>.</returns>
		public static IAsyncEnumerable<ReflectedCommand> GetAllCommandsAsync(
			this Assembly assembly,
			IServiceProvider services)
			=> assembly.GetExportedTypes().GetDirectCommandsAsync(services);

		/// <summary>
		/// Calls <see cref="GetDirectCommandsAsync(Type, IServiceProvider)"/> for
		/// every <see cref="Type"/> within <paramref name="types"/>.
		/// </summary>
		/// <param name="types">The types to gather commands from.</param>
		/// <param name="services">The services to use for dependency injection.</param>
		/// <returns>An async enumerable of <see cref="ReflectedCommand"/>.</returns>
		public static async IAsyncEnumerable<ReflectedCommand> GetDirectCommandsAsync(
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
		/// <returns>An async enumerable of <see cref="ReflectedCommand"/>.</returns>
		public static async IAsyncEnumerable<ReflectedCommand> GetDirectCommandsAsync(
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
		public readonly struct ReflectedCommand
		{
			/// <summary>
			/// The newly created command.
			/// </summary>
			public IImmutableCommand Command { get; }
			/// <summary>
			/// The <see cref="Type"/> that defined this command.
			/// </summary>
			public Type DefiningType { get; }

			/// <summary>
			/// Creates a new <see cref="ReflectedCommand"/>.
			/// </summary>
			/// <param name="definingType">
			/// <inheritdoc cref="DefiningType" path="/summary"/>
			/// </param>
			/// <param name="command">
			/// <inheritdoc cref="Command" path="/summary"/>
			/// </param>
			public ReflectedCommand(Type definingType, IImmutableCommand command)
			{
				Command = command;
				DefiningType = definingType;
			}

			/// <summary>
			/// Deconstructs this struct.
			/// </summary>
			/// <param name="definingType"><see cref="DefiningType"/></param>
			/// <param name="command"><see cref="Command"/></param>
			public void Deconstruct(out Type definingType, out IImmutableCommand command)
			{
				definingType = DefiningType;
				command = Command;
			}
		}
	}
}
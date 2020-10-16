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
				var group = ReflectionUtils.CreateInstance<ICommandGroup>(type);
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
	}

	public static class ReflectionUtils
	{
		// Some interfaces Array implements
		// Don't deal with the non generic versions b/c how would we parse 'object'?
		public static readonly ImmutableArray<Type> SupportedEnumerableTypes = new[]
		{
			typeof(IList<>),
			typeof(ICollection<>),
			typeof(IEnumerable<>),
			typeof(IReadOnlyList<>),
			typeof(IReadOnlyCollection<>),
		}.ToImmutableArray();

		public static Lazy<T> CreateDelegate<T>(Func<T> createDelegateDelegate, string name)
		{
			return new Lazy<T>(() =>
			{
				try
				{
					return createDelegateDelegate();
				}
				catch (Exception ex)
				{
					throw new ArgumentException($"Unable to create {name}.", ex);
				}
			});
		}

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

		public static Type? GetEnumerableType(this Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			if (type.IsInterface && type.IsGenericType)
			{
				var typeDefinition = type.GetGenericTypeDefinition();
				foreach (var supportedType in SupportedEnumerableTypes)
				{
					if (typeDefinition == supportedType)
					{
						return type.GetGenericArguments()[0];
					}
				}
			}
			return null;
		}

		public static (IEnumerable<PropertyInfo>, IEnumerable<FieldInfo>) GetWritableMembers(this Type type)
		{
			const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;
			var properties = type
				.GetProperties(FLAGS)
				.Where(x => x.CanWrite && x.SetMethod?.IsPublic == true);
			var fields = type
				.GetFields(FLAGS)
				.Where(x => !x.IsInitOnly);
			return (properties, fields);
		}

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

		[return: MaybeNull]
		public static T GetService<T>(this IServiceProvider provider)
			=> provider.GetService(typeof(T)) is T t ? t : default;
	}
}
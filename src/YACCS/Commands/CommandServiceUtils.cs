using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Parsing;

namespace YACCS.Commands
{
	public static class CommandServiceUtils
	{
		public static readonly ImmutableArray<CommandScore> NotFound
			= new[] { CommandScore.FromNotFound() }.ToImmutableArray();
		public static readonly ImmutableArray<CommandScore> QuoteMismatch
			= new[] { CommandScore.FromQuoteMismatch() }.ToImmutableArray();

		public static async Task<IReadOnlyList<ICommand>> CreateCommandsAsync(Type type)
		{
			const BindingFlags FLAGS = 0
				| BindingFlags.Public
				| BindingFlags.Instance
				| BindingFlags.FlattenHierarchy;

			ICommandGroup? group = null;
			List<IMutableCommand>? list = null;
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

				if (group is null || list is null)
				{
					var instance = Activator.CreateInstance(type);
					if (!(instance is ICommandGroup temp))
					{
						throw new ArgumentException($"{type.Name} does not implement {nameof(ICommandGroup)}.");
					}
					group = temp;
					list = new List<IMutableCommand>();
				}

				list.Add(new MutableCommand(group, method));
			}

			if (group != null && list != null)
			{
				await group.OnCommandBuildingAsync(list).ConfigureAwait(false);
				return list.Select(x => x.ToCommand()).ToArray();
			}
			return Array.Empty<ICommand>();
		}

		public static async IAsyncEnumerable<ICommand> GetCommandsAsync(IEnumerable<Assembly> assemblies)
		{
			foreach (var assembly in assemblies)
			{
				await foreach (var command in GetCommandsAsync(assembly))
				{
					yield return command;
				}
			}
		}

		public static async IAsyncEnumerable<ICommand> GetCommandsAsync(Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				await foreach (var command in GetCommandsAsync(type))
				{
					yield return command;
				}
			}
		}

		public static async IAsyncEnumerable<ICommand> GetCommandsAsync(Type type)
		{
			var commands = await CreateCommandsAsync(type).ConfigureAwait(false);
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

		public static IReadOnlyList<CommandScore> TryFind(this ICommandService service, string input)
		{
			if (ParseArgs.TryParse(input, out var args))
			{
				return service.TryFind(args.Arguments);
			}
			return QuoteMismatch;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Commands.Linq
{
	public interface ICommand<in TContext> : ICommand where TContext : IContext
	{
	}

	public static class Commands
	{
		public static TCommand AddName<TCommand>(this TCommand command, IReadOnlyList<string> name)
			where TCommand : ICommand
		{
			command.Names.Add(name);
			return command;
		}

		public static TCommand AddPrecondition<TContext, TCommand>(
			this TCommand command,
			IPrecondition<TContext> precondition)
			where TContext : IContext
			where TCommand : ICommand, ICommand<TContext>
		{
			command.Attributes.Add(precondition);
			return command;
		}

		public static ICommand AsCommand(this IQueryableEntity entity)
		{
			if (entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}
			if (entity is not ICommand command)
			{
				throw new ArgumentException($"Not a {nameof(ICommand)}.", nameof(entity));
			}
			return command;
		}

		public static ICommand<TContext> AsContext<TContext>(this ICommand command)
			where TContext : IContext
		{
			if (!command.IsValidContext(typeof(TContext)))
			{
				throw new ArgumentException("Is not and does not inherit or implement " +
					$"{command.ContextType!.Name}. {command.Names?.FirstOrDefault()}", nameof(command));
			}
			return new Command<TContext>(command);
		}

		public static ICommand<TContext> GetCommandById<TContext>(
			this IEnumerable<ICommand> commands,
			string id)
			where TContext : IContext
		{
			return commands
				.ById(id)
				.Single()
				.AsContext<TContext>();
		}

		public static IEnumerable<ICommand<TContext>> GetCommandsById<TContext>(
			this IEnumerable<ICommand> commands,
			string id)
			where TContext : IContext
		{
			return commands
				.ById(id)
				.Select(AsContext<TContext>);
		}

		public static IEnumerable<ICommand<TContext>> GetCommandsByType<TContext>(
			this IEnumerable<ICommand> commands)
			where TContext : IContext
		{
			foreach (var command in commands)
			{
				if (command.IsValidContext(typeof(TContext)))
				{
					yield return new Command<TContext>(command);
				}
			}
		}

		public static bool IsValidContext(this IQueryableCommand command, Type? type)
			=> command.ContextType?.IsAssignableFrom(type) ?? true;

		[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
		private sealed class Command<TContext> : ICommand<TContext> where TContext : IContext
		{
			private readonly ICommand _Actual;

			public IList<object> Attributes
			{
				get => _Actual.Attributes;
				set => _Actual.Attributes = value;
			}
			public IList<IReadOnlyList<string>> Names
			{
				get => _Actual.Names;
				set => _Actual.Names = value;
			}
			public IReadOnlyList<IParameter> Parameters
			{
				get => _Actual.Parameters;
				set => _Actual.Parameters = value;
			}
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			public Type ContextType => _Actual.ContextType;
			IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
			IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
			private string DebuggerDisplay => this.FormatForDebuggerDisplay();

			public Command(ICommand actual)
			{
				_Actual = actual;
			}

			public IImmutableCommand ToImmutable()
										=> _Actual.ToImmutable();

			public IAsyncEnumerable<IImmutableCommand> ToMultipleImmutableAsync(IServiceProvider services)
										=> _Actual.ToMultipleImmutableAsync(services);
		}
	}
}
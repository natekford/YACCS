using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Building;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public abstract class CommandGroup<TContext>
		: ICommandGroup<TContext>, IOnCommandBuilding
		where TContext : IContext
	{
		public IImmutableCommand Command { get; protected set; } = null!;
		public TContext Context { get; protected set; } = default!;

		public virtual Task AfterExecutionAsync(IImmutableCommand command, TContext context, IResult result)
			=> Task.CompletedTask;

		public virtual Task BeforeExecutionAsync(IImmutableCommand command, TContext context)
		{
			if (command is null)
			{
				throw new ArgumentNullException(nameof(command));
			}
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			Command = command;
			Context = context;
			return Task.CompletedTask;
		}

		public virtual Task ModifyCommandsAsync(IServiceProvider services, List<ReflectionCommand> commands)
		{
			Debug.WriteLine($"{GetType().Name}: {commands.Count} command(s) created.");

			var (properties, fields) = GetType().GetWritableMembers();
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

			return Task.CompletedTask;
		}

		Task ICommandGroup.AfterExecutionAsync(IImmutableCommand command, IContext context, IResult result)
		{
			if (context is null)
			{
				return AfterExecutionAsync(command, default!, result);
			}
			if (context is not TContext tContext)
			{
				throw InvalidContext(context);
			}
			return AfterExecutionAsync(command, tContext, result);
		}

		Task ICommandGroup.BeforeExecutionAsync(IImmutableCommand command, IContext context)
		{
			if (context is null)
			{
				return BeforeExecutionAsync(command, default!);
			}
			if (context is not TContext tContext)
			{
				throw InvalidContext(context);
			}
			return BeforeExecutionAsync(command, tContext);
		}

		private static ArgumentException InvalidContext(IContext context)
		{
			return new ArgumentException("Invalid context; " +
				$"expected {typeof(TContext).Name}, " +
				$"received {context.GetType().Name}.", nameof(context));
		}
	}
}
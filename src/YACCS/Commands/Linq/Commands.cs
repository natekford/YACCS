using System.Diagnostics;

using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Commands.Linq;

/// <inheritdoc />
public interface ICommand<in TContext> : ICommand where TContext : IContext;

/// <summary>
/// Static methods for querying and modifying <see cref="ICommand"/>.
/// </summary>
public static class Commands
{
	/// <summary>
	/// Adds a path to <paramref name="command"/>.
	/// </summary>
	/// <typeparam name="TCommand"></typeparam>
	/// <param name="command">The command to modify.</param>
	/// <param name="path">The path to add.</param>
	/// <returns><paramref name="command"/> after it has been modified.</returns>
	public static TCommand AddPath<TCommand>(
		this TCommand command,
		IReadOnlyList<string> path)
		where TCommand : ICommand
	{
		command.Paths.Add(path);
		return command;
	}

	/// <summary>
	/// Adds a precondition to <paramref name="command"/>.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TCommand"></typeparam>
	/// <param name="command">The command to modify.</param>
	/// <param name="precondition">The precondition to add.</param>
	/// <returns><paramref name="command"/> after it has been modified.</returns>
	public static TCommand AddPrecondition<TContext, TCommand>(
		this TCommand command,
		IPrecondition<TContext> precondition)
		where TContext : IContext
		where TCommand : ICommand, ICommand<TContext>
	{
		command.Attributes.Add(precondition);
		return command;
	}

	/// <summary>
	/// Casts <paramref name="entity"/> to <see cref="ICommand"/>.
	/// </summary>
	/// <param name="entity">The entity to cast.</param>
	/// <returns><paramref name="entity"/> cast to <see cref="ICommand"/>.</returns>
	/// <exception cref="ArgumentNullException">
	/// When <paramref name="entity"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// When <paramref name="entity"/> does not implement <see cref="ICommand"/>.
	/// </exception>
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

	/// <summary>
	/// Converts <paramref name="command"/> to a generic version.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <param name="command">The command to convert.</param>
	/// <returns>The generic version of <paramref name="command"/>.</returns>
	/// <exception cref="ArgumentException">
	/// When <typeparamref name="TContext"/> is invalid for <paramref name="command"/>.
	/// </exception>
	public static ICommand<TContext> AsContext<TContext>(this ICommand command)
		where TContext : IContext
	{
		if (!command.IsValidContext(typeof(TContext)))
		{
			throw new ArgumentException("Is not and does not inherit or implement " +
				$"{command.ContextType!.Name}. {command.Paths?.FirstOrDefault()}", nameof(command));
		}
		return new Command<TContext>(command);
	}

	/// <summary>
	/// Filters <paramref name="commands"/> by determining if <typeparamref name="TContext"/>
	/// is valid for each one.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <param name="commands">The commands to filter.</param>
	/// <returns>An enumerable of commands where <typeparamref name="TContext"/> is valid.</returns>
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

	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	private sealed class Command<TContext>(ICommand actual)
		: ICommand<TContext> where TContext : IContext
	{
		IList<object> IEntityBase.Attributes
		{
			get => actual.Attributes;
			set => actual.Attributes = value;
		}
		IEnumerable<object> IQueryableEntity.Attributes => actual.Attributes;
		Type IQueryableCommand.ContextType => actual.ContextType;
		IReadOnlyList<IParameter> ICommand.Parameters => actual.Parameters;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => actual.Parameters;
		IList<IReadOnlyList<string>> ICommand.Paths
		{
			get => actual.Paths;
			set => actual.Paths = value;
		}
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Paths => actual.Paths;
		IImmutableCommand? IQueryableCommand.Source => actual.Source;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		bool IQueryableCommand.IsValidContext(Type type)
			=> actual.IsValidContext(type);

		IImmutableCommand ICommand.ToImmutable()
			=> actual.ToImmutable();

		IAsyncEnumerable<IImmutableCommand> ICommand.ToMultipleImmutableAsync(IServiceProvider services)
			=> actual.ToMultipleImmutableAsync(services);
	}
}
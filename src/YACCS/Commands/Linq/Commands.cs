using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Commands.Linq;

/// <inheritdoc />
public interface ICommand<in TContext> : IMutableCommand where TContext : IContext;

/// <summary>
/// Static methods for querying and modifying <see cref="IMutableCommand"/>.
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
		where TCommand : IMutableCommand
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
		where TCommand : IMutableCommand, ICommand<TContext>
	{
		command.Attributes.Add(precondition);
		return command;
	}

	/// <summary>
	/// Casts <paramref name="entity"/> to <see cref="IMutableCommand"/>.
	/// </summary>
	/// <param name="entity">The entity to cast.</param>
	/// <returns><paramref name="entity"/> cast to <see cref="IMutableCommand"/>.</returns>
	/// <exception cref="ArgumentNullException">
	/// When <paramref name="entity"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// When <paramref name="entity"/> does not implement <see cref="IMutableCommand"/>.
	/// </exception>
	public static IMutableCommand AsCommand(this IQueryableEntity entity)
	{
		if (entity is null)
		{
			throw new ArgumentNullException(nameof(entity));
		}
		if (entity is not IMutableCommand command)
		{
			throw new ArgumentException($"Not a {nameof(IMutableCommand)}.", nameof(entity));
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
	public static ICommand<TContext> AsContext<TContext>(this IMutableCommand command)
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
		this IEnumerable<IMutableCommand> commands)
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
	private sealed class Command<TContext>(IMutableCommand actual)
		: ICommand<TContext> where TContext : IContext
	{
		IList<object> IMutableEntity.Attributes
		{
			get => actual.Attributes;
			set => actual.Attributes = value;
		}
		IEnumerable<object> IQueryableEntity.Attributes => actual.Attributes;
		Type IQueryableCommand.ContextType => actual.ContextType;
		IReadOnlyList<IMutableParameter> IMutableCommand.Parameters => actual.Parameters;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => actual.Parameters;
		IList<IReadOnlyList<string>> IMutableCommand.Paths => actual.Paths;
		IReadOnlyList<IReadOnlyList<string>> IQueryableCommand.Paths
			=> new ReadOnlyCollection<IReadOnlyList<string>>(actual.Paths);
		IImmutableCommand? IQueryableCommand.Source => actual.Source;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		bool IQueryableCommand.IsValidContext(Type type)
			=> actual.IsValidContext(type);

		IImmutableCommand IMutableCommand.ToImmutable()
			=> actual.ToImmutable();

		IAsyncEnumerable<IImmutableCommand> IMutableCommand.ToMultipleImmutableAsync(IServiceProvider services)
			=> actual.ToMultipleImmutableAsync(services);
	}
}
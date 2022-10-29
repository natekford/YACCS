using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Linq;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models;

/// <inheritdoc cref="ICommand"/>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public abstract class Command : EntityBase, ICommand
{
	/// <inheritdoc />
	public Type ContextType { get; protected set; }
	/// <inheritdoc />
	public IReadOnlyList<IParameter> Parameters { get; protected set; }
	/// <inheritdoc />
	public IList<IReadOnlyList<string>> Paths { get; set; }
	/// <inheritdoc />
	public IImmutableCommand? Source { get; protected set; }
	IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
	IEnumerable<IReadOnlyList<string>> IQueryableCommand.Paths => Paths;
	private string DebuggerDisplay => this.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates an instance of <see cref="Command"/>
	/// </summary>
	/// <param name="method"></param>
	/// <param name="contextType"></param>
	/// <param name="source"></param>
	protected Command(MethodInfo method, Type contextType, IImmutableCommand? source)
		: base(method)
	{
		Source = source;
		ContextType = contextType;
		Paths = new List<IReadOnlyList<string>>();
		Parameters = method.GetParameters().Select(x => new Parameter(x)).ToList<IParameter>();
	}

	/// <inheritdoc />
	public virtual bool IsValidContext(Type type)
	{
		return ContextType.IsAssignableFrom(type) && Attributes
			.OfType<IContextConstraint>()
			.All(x => x.DoesTypeSatisfy(type));
	}

	/// <inheritdoc />
	public abstract IImmutableCommand ToImmutable();

	/// <inheritdoc />
	public virtual async IAsyncEnumerable<IImmutableCommand> ToMultipleImmutableAsync(
		IServiceProvider services)
	{
		var immutable = ToImmutable();
		yield return immutable;

		foreach (var generator in this.GetAttributes<ICommandGeneratorAttribute>())
		{
			foreach (var generated in await generator.GenerateCommandsAsync(services, immutable).ConfigureAwait(false))
			{
				yield return generated;
			}
		}
	}

	/// <inheritdoc cref="IImmutableCommand"/>
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	protected abstract class ImmutableCommand : IImmutableCommand
	{
		private readonly Lazy<Func<Task, object>> _TaskResult;
		private readonly ConcurrentDictionary<Type, bool> _ValidContexts = new();

		/// <inheritdoc />
		public IReadOnlyList<object> Attributes { get; }
		/// <inheritdoc />
		public Type ContextType { get; }
		/// <inheritdoc />
		public bool IsHidden { get; }
		/// <inheritdoc />
		public int MaxLength { get; }
		/// <inheritdoc />
		public int MinLength { get; }
		/// <inheritdoc />
		public IReadOnlyList<IImmutableParameter> Parameters { get; }
		/// <inheritdoc />
		public IReadOnlyList<IReadOnlyList<string>> Paths { get; }
		/// <inheritdoc />
		public IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions { get; }
		/// <inheritdoc />
		public string PrimaryId { get; }
		/// <inheritdoc />
		public int Priority { get; }
		/// <inheritdoc />
		public IImmutableCommand? Source { get; }
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Paths => Paths;
		/// <summary>
		/// The return type for this command.
		/// </summary>
		protected Type ReturnType { get; }
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		/// <summary>
		/// Creates a new <see cref="ImmutableCommand"/>.
		/// </summary>
		/// <param name="mutable">The mutable instance of this command.</param>
		/// <param name="returnType">The return type of the object being wrapped.</param>
		protected ImmutableCommand(Command mutable, Type returnType)
		{
			ReturnType = returnType;
			ContextType = mutable.ContextType;
			Source = mutable.Source;
			_TaskResult = new(() => ReflectionUtils.CreateDelegate(TaskResult, "task result"));

			var paths = ImmutableArray.CreateBuilder<IReadOnlyList<string>>(mutable.Paths.Count);
			foreach (var path in mutable.Paths)
			{
				paths.Add(new ImmutablePath(path));
			}
			Paths = paths.MoveToImmutable();

			var parameters = ImmutableArray.CreateBuilder<IImmutableParameter>(mutable.Parameters.Count);
			for (var i = 0; i < mutable.Parameters.Count; ++i)
			{
				var immutable = mutable.Parameters[i].ToImmutable();
				parameters.Add(immutable);

				// Remainder will always be the last parameter
				if (!immutable.Length.HasValue)
				{
					if (i == mutable.Parameters.Count - 1)
					{
						MaxLength = int.MaxValue;
						break;
					}

					var orig = immutable.OriginalParameterName;
					var name = Paths?.FirstOrDefault();
					throw new InvalidOperationException($"'{orig}' from '{name}' " +
						"must be the final parameter because it is a remainder.");
				}
				if (!immutable.HasDefaultValue)
				{
					MinLength += immutable.Length.Value;
				}
				MaxLength += immutable.Length.Value;
			}
			Parameters = parameters.MoveToImmutable();

			var attributes = ImmutableArray.CreateBuilder<object>(mutable.Attributes.Count);
			// Use ConcurrentDictionary because it has GetOrAdd by default, not threading reasons
			var preconditions = new ConcurrentDictionary<string, List<IPrecondition>>();
			var p = 0;
			foreach (var attribute in mutable.Attributes)
			{
				attributes.Add(attribute);
				// No if/else in case some madman decides to implement multiple
				if (attribute is IPrecondition precondition)
				{
					preconditions.AddPrecondition(precondition);
				}
				if (attribute is IPriorityAttribute priority)
				{
					Priority = priority.ThrowIfDuplicate(x => x.Priority, ref p);
				}
				if (attribute is IIdAttribute id)
				{
					PrimaryId ??= id.Id;
				}
				if (attribute is IHiddenAttribute)
				{
					IsHidden = true;
				}
			}
			Attributes = attributes.MoveToImmutable();
			Preconditions = preconditions.ToImmutablePreconditions();

			PrimaryId ??= Guid.NewGuid().ToString();
		}

		/// <inheritdoc />
		public abstract ValueTask<IResult> ExecuteAsync(
			IContext context,
			IReadOnlyList<object?> args);

		/// <inheritdoc />
		public virtual bool IsValidContext(Type type)
		{
			return _ValidContexts.GetOrAdd(type, static (type, args) =>
			{
				return args.ContextType.IsAssignableFrom(type) && args.Attributes
					.OfType<IContextConstraint>()
					.All(x => x.DoesTypeSatisfy(type));
			}, (ContextType, Attributes));
		}

		/// <summary>
		/// Converts an <see cref="object"/> into an <see cref="IResult"/>.
		/// <br/>
		/// If the method we're wrapping is void, it returns a <see cref="Success"/>.
		/// <br/>
		/// If <paramref name="value"/> is an <see cref="IResult"/>, it returns <paramref name="value"/> directly.
		/// <br/>
		/// If <paramref name="value"/> is a <see cref="Task"/>, it calls <see cref="ConvertValueAsync(Task)"/>.
		/// <br/>
		/// Otherwise, it boxes <paramref name="value"/> with <see cref="ValueResult"/>.
		/// </summary>
		/// <param name="value">The <see cref="object"/> to convert.</param>
		/// <returns>The converted value.</returns>
		protected virtual ValueTask<IResult> ConvertValueAsync(object? value)
		{
			// Void method. No value to return, we're done
			if (ReturnType == typeof(void))
			{
				return new(Success.Instance);
			}

			// We're given a result, we can just return that
			if (value is IResult result)
			{
				return new(result);
			}

			// We're given a task
			if (value is Task task)
			{
				return new(ConvertValueAsync(task));
			}

			// Essentially box random values
			return new(new ValueResult(value));
		}

		/// <summary>
		/// Converts a <see cref="Task"/> into an <see cref="IResult"/>.
		/// </summary>
		/// <param name="task">The <see cref="Task"/> to convert.</param>
		/// <returns>The converted value.</returns>
		protected virtual async Task<IResult> ConvertValueAsync(Task task)
		{
			// Let's await it to actually complete it
			await task.ConfigureAwait(false);

			// Not generic? No value to return, we're done
			if (!ReturnType.IsGenericType)
			{
				return Success.Instance;
			}

			// It has a value? Ok, let's get it
			var value = _TaskResult.Value.Invoke(task);
			return await ConvertValueAsync(value).ConfigureAwait(false);
		}

		private Func<Task, object> TaskResult()
		{
			/*
			 *	(Task Task) =>
			 *	{
			 *		return ((Task<T>)Task).Result;
			 *	}
			 */

			var instance = Expression.Parameter(typeof(Task), "Task");

			var instanceCast = Expression.Convert(instance, ReturnType);
			var property = Expression.Property(instanceCast, nameof(Task<object>.Result));
			var propertyCast = Expression.Convert(property, typeof(object));

			var lambda = Expression.Lambda<Func<Task, object>>(
				propertyCast,
				instance
			);
			return lambda.Compile();
		}
	}
}
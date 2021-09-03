using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public sealed class ReflectionCommand : Command
	{
		public Type GroupType { get; }
		public MethodInfo Method { get; }

		public ReflectionCommand(MethodInfo method) : this(method, null)
		{
		}

		public ReflectionCommand(MethodInfo method, IImmutableCommand? source)
			: this(method, method.ReflectedType, source)
		{
		}

		private ReflectionCommand(
			MethodInfo method,
			Type groupType,
			IImmutableCommand? source)
			: base(method, GetContextType(groupType), source)
		{
			GroupType = groupType;
			Method = method;

			Attributes.Add(Method);

			// Add in all names
			foreach (var name in GetFullNames(groupType, method))
			{
				Names.Add(name);
			}

			// Add in all attributes
			var type = groupType;
			while (type is not null)
			{
				foreach (var attribute in type.GetCustomAttributes(true))
				{
					Attributes.Add(attribute);
				}
				type = type.DeclaringType;
			}
		}

		public override IImmutableCommand ToImmutable()
			=> new ImmutableReflectionCommand(this);

		private static Type GetContextType(Type groupType)
		{
			if (groupType.GetConstructor(Type.EmptyTypes) is null)
			{
				throw new ArgumentException(
					$"{groupType.FullName} is missing a public parameterless constructor", nameof(groupType));
			}

			foreach (var @interface in groupType.GetInterfaces())
			{
				if (@interface.IsGenericOf(typeof(ICommandGroup<>)))
				{
					return @interface.GetGenericArguments().Single();
				}
			}
			throw new ArgumentException(
				$"{groupType.FullName} must implement {typeof(ICommandGroup<>).FullName}.", nameof(groupType));
		}

		private static IEnumerable<IReadOnlyList<string>> GetFullNames(
			Type group,
			MethodInfo method)
		{
			var names = method
				.GetCustomAttributes(true)
				.OfType<ICommandAttribute>()
				.SingleOrDefault()
				?.Names ?? Enumerable.Empty<string>();

			var output = new List<IEnumerable<string>>(names.Select(x => new[] { x }));
			if (output.Count == 0)
			{
				output.Add(Enumerable.Empty<string>());
			}

			var parent = group;
			while (parent is not null)
			{
				var command = parent
					.GetCustomAttributes()
					.OfType<ICommandAttribute>()
					.SingleOrDefault();
				if (command is not null)
				{
					var count = output.Count;
					for (var i = 0; i < count; ++i)
					{
						foreach (var name in command.Names)
						{
							output.Add(output[i].Prepend(name));
						}
					}
					output.RemoveRange(0, count);
				}
				parent = parent.DeclaringType;
			}

			return output.Select(x => new ImmutableName(x));
		}

		private sealed class ImmutableReflectionCommand : ImmutableCommand
		{
			private static readonly ConcurrentDictionary<Type, Func<IContext, ICommandGroup>> _ConstructorCache = new();

			private readonly Func<IContext, ICommandGroup> _Constructor;
			private readonly Func<ICommandGroup, object?[], object> _Execute;
			private readonly Type _GroupType;
			private readonly MethodInfo _Method;

			public ImmutableReflectionCommand(ReflectionCommand mutable)
				: base(mutable, mutable.Method.ReturnType)
			{
				_GroupType = mutable.GroupType;
				_Method = mutable.Method;

				_Constructor = ReflectionUtils.CreateDelegate(Constructor, "constructor + injection");
				_Execute = ReflectionUtils.CreateDelegate(Execute, "execute");
			}

			public override async ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
			{
				// Don't catch exceptions in here, it's easier for the command handler to
				// catch them itself + this makes testing easier

				var group = _Constructor.Invoke(context);
				await group.BeforeExecutionAsync(this, context).ConfigureAwait(false);

				var value = _Execute.Invoke(group, args);
				var result = await ConvertValueAsync(value).ConfigureAwait(false);
				await group.AfterExecutionAsync(this, context, result).ConfigureAwait(false);

				return result;
			}

			private Func<IContext, ICommandGroup> Constructor()
			{
				/*
				 *	(IContext Context) =>
				 *	{
				 *		var provider = Context.Provider;
				 *		var group = new GroupType();
				 *
				 *		((DeclaringType)group).ContextInterface = (ContextInterface)Context;
				 *
				 *		try
				 *		{
				 *			var __var = Provider.GetService(typeof(ServiceA));
				 *
				 *			if (__var is ServiceA)
				 *			{
				 *				((DeclaringType)group).ServiceA = (ServiceA)__var;
				 *			}
				 *		}
				 *		catch (Exception e)
				 *		{
				 *			throw new ArgumentException(message, nameof(DeclaringType.ServiceA), e);
				 *		}
				 *
				 *		return group;
				 *	}
				 */

				return _ConstructorCache.GetOrAdd(_GroupType, groupType =>
				{
					var context = Expression.Parameter(typeof(IContext), "Context");

					var group = Expression.Variable(groupType, "group");
					var @new = Expression.New(groupType);
					var assignGroup = Expression.Assign(group, @new);

					var setters = groupType.SelectWritableMembers(group, member =>
					{
						var injector = member.Member.GetCustomAttribute<InjectableAttribute>();
						if (injector is null)
						{
							throw new InvalidOperationException("Unable to find a specified way " +
								$"to set the public property '{member.Member}' for '{member.Member.ReflectedType}'.");
						}
						return injector.CreateInjection(context, member);
					});
					var body = Expression.Block(new[]
					{
						group
					}, setters.Prepend(assignGroup).Append(group));

					var lambda = Expression.Lambda<Func<IContext, ICommandGroup>>(
						body,
						context
					);
					return lambda.Compile();
				});
			}

			private Func<ICommandGroup, object?[], object> Execute()
			{
				return ExpressionUtils.GetInvokeFromObjectArray<Func<ICommandGroup, object?[], object>>(
					Expression.Parameter(typeof(ICommandGroup), "Group"),
					_Method
				);
			}
		}
	}
}
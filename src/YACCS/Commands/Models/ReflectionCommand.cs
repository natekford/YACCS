using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.NamedArguments;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public class ReflectionCommand : Command
	{
		public override Type? ContextType { get; }
		public Type GroupType { get; }
		public MethodInfo Method { get; }

		public ReflectionCommand(MethodInfo method, IEnumerable<string>? extraNames = null)
			: base(method)
		{
			var group = method.ReflectedType;
			if (!typeof(ICommandGroup).IsAssignableFrom(group))
			{
				throw new ArgumentException($"Must implement {typeof(ICommandGroup).FullName}.", nameof(group));
			}
			if (group.GetConstructor(Type.EmptyTypes) == null)
			{
				throw new ArgumentException("Missing a public parameterless constructor.", nameof(group));
			}

			ContextType = group
				.GetInterfaces()
				.SingleOrDefault(x => x.IsGenericOf(typeof(ICommandGroup<>)))
				?.GetGenericArguments()
				?.Single();
			GroupType = group;
			Method = method;

			foreach (var name in GetFullNames(group, method, extraNames))
			{
				Names.Add(name);
			}
			AddAllParentsAttributes(group);

			Attributes.Add(new MethodInfoCommandAttribute(Method));
		}

		public override IEnumerable<IImmutableCommand> ToImmutable()
		{
			var immutable = new ImmutableReflectionCommand(this);
			return this.Get<GenerateNamedArgumentsAttribute>().Any()
				? new[] { immutable, immutable.GenerateNamedArgumentVersion() }
				: new[] { immutable };
		}

		private static IEnumerable<IReadOnlyList<string>> GetFullNames(
			Type group,
			MethodInfo method,
			IEnumerable<string>? extraNames)
		{
			var names = method
				.GetCustomAttributes(true)
				.OfType<ICommandAttribute>()
				.SingleOrDefault()
				?.Names ?? Enumerable.Empty<string>();
			if (extraNames is not null)
			{
				names = names.Concat(extraNames);
			}

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

		private void AddAllParentsAttributes(Type type)
		{
			while (type is not null)
			{
				AddAttributes(type);
				type = type.DeclaringType;
			}
		}

		protected class ImmutableReflectionCommand : ImmutableCommand
		{
			private static readonly MethodInfo _GetService = typeof(IServiceProvider)
				.GetMethod(nameof(IServiceProvider.GetService));

			private readonly Lazy<Func<ICommandGroup>> _ConstructorDelegate;
			private readonly Type _GroupType;
			private readonly Lazy<Action<ICommandGroup, IServiceProvider>> _InjectionDelegate;
			private readonly Lazy<Func<ICommandGroup, object?[], object>> _InvokeDelegate;
			private readonly MethodInfo _Method;

			public ImmutableReflectionCommand(ReflectionCommand mutable)
				: base(mutable, mutable.Method.ReturnType)
			{
				_GroupType = mutable.GroupType;
				_Method = mutable.Method;

				_ConstructorDelegate = ReflectionUtils.CreateDelegate(CreateConstructorDelegate,
					"constructor delegate");
				_InjectionDelegate = ReflectionUtils.CreateDelegate(CreateInjectionDelegate,
					"injection delegate");
				_InvokeDelegate = ReflectionUtils.CreateDelegate(CreateInvokeDelegate,
					"invoke delegate");
			}

			public override async Task<IResult> ExecuteAsync(IContext context, object?[] args)
			{
				// Don't catch exceptions in here, it's easier for the command handler to
				// catch them itself + this makes testing easier

				var group = _ConstructorDelegate.Value.Invoke();
				_InjectionDelegate.Value.Invoke(group, context.Services);
				await group.BeforeExecutionAsync(this, context).ConfigureAwait(false);

				var value = _InvokeDelegate.Value.Invoke(group, args);
				var result = await ConvertValueAsync(value).ConfigureAwait(false);

				await group.AfterExecutionAsync(this, context).ConfigureAwait(false);
				return result;
			}

			protected virtual Func<ICommandGroup> CreateConstructorDelegate()
			{
				var ctor = Expression.New(_GroupType.GetConstructor(Type.EmptyTypes));

				var lambda = Expression.Lambda<Func<ICommandGroup>>(
					ctor
				);
				return lambda.Compile();
			}

			protected virtual Action<ICommandGroup, IServiceProvider> CreateInjectionDelegate()
			{
				/*
				 *	(ICommandGroup Group, IServiceProvider Provider) =>
				 *	{
				 *		object? Var0 = Provider.GetService(typeof(ServiceA));
				 *		if (Var0 is ServiceA)
				 *		{
				 *			((DeclaringType)Group).ServiceA = (ServiceA)Var0;
				 *		}
				 *		object? Var1 = Provider.GetService(typeof(ServiceB));
				 *		if (Var1 is ServiceB)
				 *		{
				 *			((DeclaringType)Group).ServiceB = (ServiceB)Var1;
				 *		}
				 *	}
				 */

				var instance = Expression.Parameter(typeof(ICommandGroup), "Group");
				var provider = Expression.Parameter(typeof(IServiceProvider), "Provider");

				var memberCount = 0;
				var setters = _GroupType.CreateExpressionsForWritableMembers<Expression>(instance, x =>
				{
					// Create temp variable
					var typeArgument = Expression.Constant(x.Type);
					var service = Expression.Call(provider, _GetService, typeArgument);
					var temp = Expression.Variable(typeof(object), $"__var{memberCount++}");
					var tempAssign = Expression.Assign(temp, service);

					// Make sure the temp variable is not null
					var isType = Expression.TypeIs(temp, x.Type);

					// Set member to temp variable
					var serviceCast = Expression.Convert(temp, x.Type);
					var assign = Expression.Assign(x, serviceCast);

					var ifThen = Expression.IfThen(isType, assign);
					var body = Expression.Block(new[] { temp }, tempAssign, ifThen);

					// Catch any exceptions and throw a more informative one
					var message = $"Failed setting a service for {_GroupType.FullName}.";
					return body.AddThrow((Exception e) => new ArgumentException(message, x.Member.Name, e));
				});
				var body = Expression.Block(setters);

				var lambda = Expression.Lambda<Action<ICommandGroup, IServiceProvider>>(
					body,
					instance,
					provider
				);
				return lambda.Compile();
			}

			protected virtual Func<ICommandGroup, object?[], object> CreateInvokeDelegate()
			{
				/*
				 *	(ICommandGroup Group, object?[] Args) =>
				 *	{
				 *		return ((DeclaringType)Group).Method((ParamType)Args[0], (ParamType)Args[1], ...);
				 *	}
				 */

				var instance = Expression.Parameter(typeof(ICommandGroup), "Group");

				var (body, args) = instance.CreateInvokeExpressionFromObjectArrayArgs(_Method);

				var lambda = Expression.Lambda<Func<ICommandGroup, object?[], object>>(
					body,
					instance,
					args
				);
				return lambda.Compile();
			}
		}
	}
}
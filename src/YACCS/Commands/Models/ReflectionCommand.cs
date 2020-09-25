using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public sealed class ReflectionCommand : Command
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

			ContextType = group.GetInterfaces().SingleOrDefault(x =>
			{
				return x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandGroup);
			});
			GroupType = group;
			Method = method;

			foreach (var name in GetFullNames(group, method, extraNames))
			{
				Names.Add(name);
			}
			AddAllParentsAttributes(group);

			Attributes.Add(new MethodInfoCommandAttribute(Method));
		}

		public override IImmutableCommand ToCommand()
			=> new ImmutableReflectionCommand(this);

		private static IList<IName> GetFullNames(
			Type group,
			MethodInfo method,
			IEnumerable<string>? extraNames)
		{
			var names = Enumerable.Empty<string>();
			var methodNames = method
				.GetCustomAttributes(true)
				.OfType<ICommandAttribute>()
				.SingleOrDefault()
				?.Names;
			if (methodNames != null)
			{
				names = names.Concat(methodNames);
			}
			if (extraNames != null)
			{
				names = names.Concat(extraNames);
			}

			var output = new List<IEnumerable<string>>(names.Select(x => new[] { x }));
			if (output.Count == 0)
			{
				output.Add(Enumerable.Empty<string>());
			}

			var type = group;
			while (type != null)
			{
				var command = type
					.GetCustomAttributes()
					.OfType<ICommandAttribute>()
					.SingleOrDefault();
				if (command != null)
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
				type = type.DeclaringType;
			}

			return output.Select(x => new Name(x)).ToList<IName>();
		}

		private void AddAllParentsAttributes(Type type)
		{
			while (type != null)
			{
				AddAttributes(type);
				type = type.DeclaringType;
			}
		}

		private sealed class ImmutableReflectionCommand : ImmutableCommand
		{
			private readonly Lazy<Func<ICommandGroup>> _ConstructorDelegate;
			private readonly Type? _ContextType;
			private readonly Type _GroupType;
			private readonly Lazy<Action<ICommandGroup, IServiceProvider>> _InjectionDelegate;
			private readonly Lazy<Func<ICommandGroup, object?[], object>> _InvokeDelegate;
			private readonly MethodInfo _Method;

			public ImmutableReflectionCommand(ReflectionCommand mutable)
				: base(mutable, mutable.Method.ReturnType)
			{
				_ContextType = mutable.ContextType;
				_GroupType = mutable.GroupType;
				_Method = mutable.Method;

				_ConstructorDelegate = CreateDelegate(CreateConstructorDelegate, "constructor delegate");
				_InjectionDelegate = CreateDelegate(CreateInjectionDelegate, "injection delegate");
				_InvokeDelegate = CreateDelegate(CreateInvokeDelegate, "invoke delegate");
			}

			public override async Task<ExecutionResult> ExecuteAsync(IContext context, object?[] args)
			{
				// Don't catch exceptions in here, it's easier for the command handler to
				// catch them itself + this makes testing easier

				var group = _ConstructorDelegate.Value.Invoke();
				_InjectionDelegate.Value.Invoke(group, context.Services);
				await group.BeforeExecutionAsync(this, context).ConfigureAwait(false);

				var value = _InvokeDelegate.Value.Invoke(group, args);
				var result = await ConvertValueAsync(context, value).ConfigureAwait(false);

				await group.AfterExecutionAsync(this, context).ConfigureAwait(false);
				return result;
			}

			private Func<ICommandGroup> CreateConstructorDelegate()
			{
				var ctor = _GroupType.GetConstructor(Type.EmptyTypes);
				var ctorExpr = Expression.New(ctor);
				var lambda = Expression.Lambda<Func<ICommandGroup>>(ctorExpr);
				return lambda.Compile();
			}

			private Action<ICommandGroup, IServiceProvider> CreateInjectionDelegate()
			{
				/*
				 *	(ICommandGroup Group, IServiceProvider Provider) =>
				 *	{
				 *		object? serviceA = Provider.GetService(typeof(ServiceA));
				 *		if (serviceA != null)
				 *		{
				 *			((GroupType)Group).ServiceA = (ServiceA)serviceA;
				 *		}
				 *		object? serviceB = Provider.GetService(typeof(ServiceB));
				 *		if (serviceB != null)
				 *		{
				 *			((GroupType)Group).ServiceB = (ServiceB)serviceA;
				 *		}
				 *	}
				 */

				const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;
				var properties = _GroupType
					.GetProperties(FLAGS)
					.Where(x => x.CanWrite && x.SetMethod?.IsPublic == true);
				var fields = _GroupType
					.GetFields(FLAGS)
					.Where(x => !x.IsInitOnly);
				var getService =
					typeof(IServiceProvider)
					.GetMethod(nameof(IServiceProvider.GetService));

				var instanceExpr = Expression.Parameter(typeof(ICommandGroup), "Group");
				var providerExpr = Expression.Parameter(typeof(IServiceProvider), "Provider");
				var instanceCastExpr = Expression.Convert(instanceExpr, _GroupType);
				var nullExpr = Expression.Constant(null);

				ConditionalExpression CreateExpression(
					Type type,
					Func<UnaryExpression?, MemberExpression> memberGetter)
				{
					var typeExpr = Expression.Constant(type);
					var serviceExpr = Expression.Call(providerExpr, getService, typeExpr);
					var serviceCastExpr = Expression.Convert(serviceExpr, type);

					var memberExpr = memberGetter(instanceCastExpr);
					var assignExpr = Expression.Assign(memberExpr, serviceCastExpr);

					var notNullExpr = Expression.NotEqual(nullExpr, serviceCastExpr);
					return Expression.IfThen(notNullExpr, assignExpr);
				}

				var propertyExprs = properties.Select(x =>
				{
					return CreateExpression(x.PropertyType, u =>
					{
						return Expression.Property(u, x.SetMethod);
					});
				});
				var fieldExprs = fields.Select(x =>
				{
					return CreateExpression(x.FieldType, u =>
					{
						return Expression.Field(u, x);
					});
				});
				var allAssignExpr = Expression.Block(propertyExprs.Concat(fieldExprs));

				var lambda = Expression.Lambda<Action<ICommandGroup, IServiceProvider>>(
					allAssignExpr,
					instanceExpr,
					providerExpr
				);
				return lambda.Compile();
			}

			private Func<ICommandGroup, object?[], object> CreateInvokeDelegate()
			{
				/*
				 *	(ICommandGroup Group, object?[] Args) =>
				 *	{
				 *		return ((GroupType)Group).Method((ParamType)Args[0], (ParamType)Args[1], ...);
				 *	}
				 */

				var instanceExpr = Expression.Parameter(typeof(ICommandGroup), "Group");
				var argsExpr = Expression.Parameter(typeof(object?[]), "Args");

				var instanceCastExpr = Expression.Convert(instanceExpr, _GroupType);
				var argsCastExpr = _Method.GetParameters().Select((x, i) =>
				{
					var indexExpr = Expression.Constant(i);
					var accessExpr = Expression.ArrayAccess(argsExpr, indexExpr);
					return Expression.Convert(accessExpr, x.ParameterType);
				});
				var invokeExpr = Expression.Call(instanceCastExpr, _Method, argsCastExpr);

				var lambda = Expression.Lambda<Func<ICommandGroup, object?[], object>>(
					invokeExpr,
					instanceExpr,
					argsExpr
				);
				return lambda.Compile();
			}
		}
	}
}
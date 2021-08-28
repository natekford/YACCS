﻿using System;
using System.Collections.Concurrent;
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

			foreach (var name in GetFullNames(groupType, method))
			{
				Names.Add(name);
			}
			AddAllParentsAttributes(groupType);

			Attributes.Add(Method);
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

		private void AddAllParentsAttributes(Type type)
		{
			while (type is not null)
			{
				foreach (var attribute in type.GetCustomAttributes(true))
				{
					Attributes.Add(attribute);
				}
				type = type.DeclaringType;
			}
		}

		private sealed class ImmutableReflectionCommand : ImmutableCommand
		{
			private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, ICommandGroup>> _ConstructorCache = new();
			private static readonly MethodInfo _GetService = typeof(IServiceProvider)
				.GetMethod(nameof(IServiceProvider.GetService));

			private readonly Func<IServiceProvider, ICommandGroup> _Constructor;
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

				var group = _Constructor.Invoke(context.Services);
				await group.BeforeExecutionAsync(this, context).ConfigureAwait(false);

				var value = _Execute.Invoke(group, args);
				var result = await ConvertValueAsync(value).ConfigureAwait(false);
				await group.AfterExecutionAsync(this, context, result).ConfigureAwait(false);

				return result;
			}

			private Func<IServiceProvider, ICommandGroup> Constructor()
			{
				/*
				 *	(IServiceProvider Provider) =>
				 *	{
				 *		var group = new GroupType();
				 *		try
				 *		{
				 *			var __var0 = Provider.GetService(typeof(ServiceA));
				 *
				 *			if (__var0 is ServiceA)
				 *			{
				 *				((DeclaringType)group).ServiceA = (ServiceA)__var0;
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
					var provider = Expression.Parameter(typeof(IServiceProvider), "Provider");

					var group = Expression.Variable(groupType, "group");
					var @new = Expression.New(groupType);
					var assignGroup = Expression.Assign(group, @new);

					var setters = groupType.CreateExpressionsForWritableMembers<Expression>(group, x =>
					{
						// Create temp variable
						var typeArgument = Expression.Constant(x.Type);
						var getService = Expression.Call(provider, _GetService, typeArgument);
						var temp = Expression.Variable(typeof(object), "__var");
						var tempAssign = Expression.Assign(temp, getService);

						// Make sure the temp variable is not null
						var isType = Expression.TypeIs(temp, x.Type);

						// Set member to temp variable
						var serviceCast = Expression.Convert(temp, x.Type);
						var assignService = Expression.Assign(x, serviceCast);

						var ifThen = Expression.IfThen(isType, assignService);
						var body = Expression.Block(new[] { temp }, tempAssign, ifThen);

						// Catch any exceptions and throw a more informative one
						var message = $"Failed setting a service for {groupType.FullName}.";
						return body.AddThrow((Exception e) => new ArgumentException(message, x.Member.Name, e));
					});
					var body = Expression.Block(new[] { group }, setters.Prepend(assignGroup).Append(group));

					var lambda = Expression.Lambda<Func<IServiceProvider, ICommandGroup>>(
						body,
						provider
					);
					return lambda.Compile();
				});
			}

			private Func<ICommandGroup, object?[], object> Execute()
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
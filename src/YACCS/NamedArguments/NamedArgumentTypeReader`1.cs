using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.NamedArguments
{
	public class NamedArgumentTypeReader<T> : TypeReader<T> where T : new()
	{
		private static readonly char[] _TrimEndChars = new[] { ':' };
		private static readonly char[] _TrimStartChars = new[] { '/', '-' };
		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;
		private readonly Lazy<Action<T, string, object>> _Setter;

		protected virtual IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;
		protected virtual Action<T, string, object> Setter => _Setter.Value;

		public NamedArgumentTypeReader()
		{
			_Parameters = new Lazy<IReadOnlyDictionary<string, IImmutableParameter>>(() =>
			{
				return NamedArgumentUtils.CreateParameters<T>()
					.ToParameterDictionary(x => x.OverriddenParameterName);
			});
			_Setter = ReflectionUtils.CreateDelegate(CreateSetterDelegate,
				"setter delegate");
		}

		public override ITask<ITypeReaderResult<T>> ReadAsync(IContext context, string input)
		{
			if (!ParseArgs.TryParse(
				input,
				CommandServiceUtils.InternallyUsedQuotes,
				CommandServiceUtils.InternallyUsedQuotes,
				CommandServiceUtils.InternallyUsedSeparator,
				out var args) || !TryCreateDict(args, out var dict))
			{
				return TypeReaderResult<T>.Failure.ITask;
			}
			return ReadDictIntoInstanceAsync(context, dict);
		}

		protected virtual bool TryCreateDict(ParseArgs args, [NotNullWhen(true)] out IDictionary<string, string>? dict)
		{
			if (args.Arguments.Count % 2 != 0)
			{
				dict = null;
				// TODO: more descriptive error
				return false;
			}

			dict = new Dictionary<string, string>();
			for (var i = 0; i < args.Arguments.Count; i += 2)
			{
				var name = args.Arguments[i].TrimStart(_TrimStartChars).TrimEnd(_TrimEndChars);
				if (!Parameters.TryGetValue(name, out var parameter))
				{
					// TODO: more descriptive error
					return false;
				}
				if (dict.ContainsKey(parameter.OverriddenParameterName))
				{
					// TODO: more descriptive error
					return false;
				}
				dict.Add(parameter.OverriddenParameterName, args.Arguments[i + 1]);
			}
			return true;
		}

		private static Action<T, string, object> CreateSetterDelegate()
		{
			/*
			 *	(T Instance, string Name, object Value) =>
			 *	{
			 *		if (Name == "MemberA")
			 *		{
			 *			Instance.MemberA = (A)Value;
			 *			return;
			 *		}
			 *		if (Name == "MemberB")
			 *		{
			 *			Instance.MemberB = (B)Value;
			 *			return;
			 *		}
			 *		if (Name == "MemberC")
			 *		{
			 *			Instance.MemberC = (C)Value;
			 *			return;
			 *		}
			 *	}
			 */

			var instanceExpr = Expression.Parameter(typeof(T), "Instance");
			var nameExpr = Expression.Parameter(typeof(string), "Name");
			var valueExpr = Expression.Parameter(typeof(object), "Value");
			var returnLabel = Expression.Label();

			var (properties, fields) = typeof(T).GetWritableMembers();

			Expression CreateExpression(
				string name,
				Type type,
				Func<ParameterExpression?, MemberExpression> memberGetter)
			{
				var typeExpr = Expression.Constant(type);
				var valueCastExpr = Expression.Convert(valueExpr, type);

				var memberExpr = memberGetter(instanceExpr);
				var assignExpr = Expression.Assign(memberExpr, valueCastExpr);

				var returnExpr = Expression.Return(returnLabel);
				var bodyExpr = Expression.Block(assignExpr, returnExpr);

				var memberNameExpr = Expression.Constant(name);
				var isMemberExpr = Expression.Equal(memberNameExpr, nameExpr);
				return Expression.IfThen(isMemberExpr, bodyExpr);
			}

			var propertyExprs = properties.Select(x =>
			{
				return CreateExpression(x.Name, x.PropertyType, u =>
				{
					return Expression.Property(u, x.SetMethod);
				});
			});
			var fieldExprs = fields.Select(x =>
			{
				return CreateExpression(x.Name, x.FieldType, u =>
				{
					return Expression.Field(u, x);
				});
			});
			var expressions = propertyExprs
				.Concat(fieldExprs)
				.Append(Expression.Label(returnLabel));
			var allAssignExpr = Expression.Block(expressions);

			var lambda = Expression.Lambda<Action<T, string, object>>(
				allAssignExpr,
				instanceExpr,
				nameExpr,
				valueExpr
			);
			return lambda.Compile();
		}

		private async ITask<ITypeReaderResult<T>> ReadDictIntoInstanceAsync(
			IContext context,
			IDictionary<string, string> dict)
		{
			var registry = context.Services.GetRequiredService<ITypeReaderRegistry>();
			var instance = new T();
			foreach (var kvp in dict)
			{
				var parameter = Parameters[kvp.Key];
				var reader = registry.GetReader(parameter.ParameterType);

				var trResult = await reader.ReadAsync(context, kvp.Value).ConfigureAwait(false);
				if (!trResult.IsSuccess)
				{
					// TODO: return a more descriptive error based on the member failed
					return TypeReaderResult<T>.Failure.Sync;
				}

				Setter.Invoke(instance, parameter.ParameterName, trResult.Value!);
			}
			return TypeReaderResult<T>.FromSuccess(instance);
		}
	}
}
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

namespace YACCS.TypeReaders
{
	public class NamedTypeReader<T> : TypeReader<T> where T : new()
	{
		private static readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;
		private static readonly Lazy<Action<T, string, object>> _Setter;
		private static readonly Lazy<IImmutableCommand> _SetterCommand;

		private static readonly char[] _TrimEndChars = new[] { ':' };
		private static readonly char[] _TrimStartChars = new[] { '/', '-' };

		static NamedTypeReader()
		{
			_SetterCommand = new Lazy<IImmutableCommand>(() =>
			{
				var names = new[] { new Name(new[] { "__NamedTypeReaderSetter" }) };
				var command = new DelegateCommand(_Setter.Value, names);
				command.Parameters.Clear();

				var (properties, fields) = typeof(T).GetWritableMembers();
				var parameters = properties
					.Select(x => new Parameter(x))
					.Concat(fields.Select(x => new Parameter(x)));
				foreach (var parameter in parameters)
				{
					command.Parameters.Add(parameter);
				}

				return command.ToCommand();
			});
			_Parameters = new Lazy<IReadOnlyDictionary<string, IImmutableParameter>>(() =>
			{
				return _SetterCommand
					.Value
					.Parameters
					.ToDictionary(x => x.OverriddenParameterName, x => x, StringComparer.OrdinalIgnoreCase);
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

		private static Action<T, string, object> CreateSetterDelegate()
		{
			/*
			 *	(T Instance, string Member, object Value) =>
			 *	{
			 *		if (Member == "MemberA")
			 *		{
			 *			Instance.MemberA = (A)Value;
			 *			return;
			 *		}
			 *		if (Member == "MemberB")
			 *		{
			 *			Instance.MemberB = (B)Value;
			 *			return;
			 *		}
			 *		if (Member == "MemberC")
			 *		{
			 *			Instance.MemberC = (C)Value;
			 *			return;
			 *		}
			 *	}
			 */

			var instanceExpr = Expression.Parameter(typeof(T), "Instance");
			var memberExpr = Expression.Parameter(typeof(string), "Member");
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
				var isMemberExpr = Expression.Equal(memberNameExpr, memberExpr);
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
				memberExpr,
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
				var parameter = _Parameters.Value[kvp.Key];
				var reader = registry.GetReader(parameter.ParameterType);

				var trResult = await reader.ReadAsync(context, kvp.Value).ConfigureAwait(false);
				if (!trResult.IsSuccess)
				{
					// TODO: return a more descriptive error based on the member failed
					return TypeReaderResult<T>.Failure.Sync;
				}
				var value = trResult.Value!;

				var parameterInfo = new ParameterInfo(_SetterCommand.Value, parameter);
				foreach (var precondition in parameter.Preconditions)
				{
					var pResult = await precondition.CheckAsync(parameterInfo, context, value).ConfigureAwait(false);
					if (!pResult.IsSuccess)
					{
						// TODO: return a more descriptive error based on the failed precondition
						return TypeReaderResult<T>.Failure.Sync;
					}
				}

				_Setter.Value.Invoke(instance, parameter.ParameterName, value);
			}
			return TypeReaderResult<T>.FromSuccess(instance);
		}

		private bool TryCreateDict(ParseArgs args, [NotNullWhen(true)] out IDictionary<string, string>? dict)
		{
			if (args.Arguments.Count % 2 != 0)
			{
				dict = null;
				return false;
			}

			dict = new Dictionary<string, string>();
			for (var i = 0; i < args.Arguments.Count; i += 2)
			{
				var name = args.Arguments[i].TrimStart(_TrimStartChars).TrimEnd(_TrimEndChars);
				if (!_Parameters.Value.TryGetValue(name, out var parameter))
				{
					return false;
				}
				dict.Add(parameter.ParameterName, args.Arguments[i + 1]);
			}
			return true;
		}
	}
}
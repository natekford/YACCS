using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public static class NamedTypeReaderUtils
	{
		public static NamedTypeReader<T> Create<T>() where T : new()
			=> new NamedTypeReader<T>(() => new T());

		public static IImmutableCommand GenerateNamedArgumentVersion(this IImmutableCommand command)
		{
			Task<ExecutionResult> ExecuteAsync(IContext context, IDictionary<string, object?> values)
			{
				var args = new object?[command.Parameters.Count];
				for (var i = 0; i < args.Length; ++i)
				{
					var parameter = command.Parameters[i];
					if (values.TryGetValue(parameter.ParameterName, out var value))
					{
						args[i] = value;
					}
					else if (parameter.HasDefaultValue)
					{
						args[i] = parameter.DefaultValue;
					}
					else
					{
						throw new InvalidOperationException("Generated named argument commands cannot handle missing values.");
					}
				}
				return command.ExecuteAsync(context, args);
			}

			var @delegate = (Func<IContext, IDictionary<string, object?>, Task<ExecutionResult>>)ExecuteAsync;
			var newCommand = new DelegateCommand(@delegate, command.ContextType, command.Names);
			newCommand.AddAttribute(new GeneratedNamedArgumentsAttribute(command));

			var contextParameter = newCommand
				.Parameters
				.Single(x => x.ParameterType == typeof(IContext));
			contextParameter.AddAttribute(new ContextAttribute());

			var valuesParameter = newCommand
				.Parameters
				.Single(x => x.ParameterType == typeof(IDictionary<string, object?>));
			valuesParameter.AddAttribute(new RemainderAttribute());
			valuesParameter.OverriddenTypeReader = new GeneratedNamedTypeReader(command);

			return newCommand.ToCommand();
		}

		public class GeneratedNamedTypeReader : NamedTypeReader<IDictionary<string, object?>>
		{
			protected override IImmutableCommand Command { get; }
			protected override Action<IDictionary<string, object?>, string, object> Setter { get; }

			public GeneratedNamedTypeReader(IImmutableCommand command)
				: base(() => new Dictionary<string, object?>())
			{
				Setter = (dict, key, value) => dict[key] = value;
				Command = command;
			}

			protected override bool TryCreateDict(ParseArgs args, [NotNullWhen(true)] out IDictionary<string, string>? dict)
			{
				if (!base.TryCreateDict(args, out dict))
				{
					return false;
				}

				foreach (var kvp in Parameters)
				{
					if (!dict.ContainsKey(kvp.Key) && !kvp.Value.HasDefaultValue)
					{
						return false;
					}
				}
				return true;
			}
		}
	}

	public class NamedTypeReader<T> : TypeReader<T>
	{
		private static readonly char[] _TrimEndChars = new[] { ':' };
		private static readonly char[] _TrimStartChars = new[] { '/', '-' };
		private readonly Lazy<IImmutableCommand> _Command;
		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;
		private readonly Lazy<Action<T, string, object>> _Setter;
		private readonly Func<T> CreateInstance;

		protected virtual IImmutableCommand Command => _Command.Value;
		protected virtual IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;
		protected virtual Action<T, string, object> Setter => _Setter.Value;

		public NamedTypeReader(Func<T> createInstance)
		{
			CreateInstance = createInstance;

			_Parameters = new Lazy<IReadOnlyDictionary<string, IImmutableParameter>>(() =>
			{
				return Command
					.Parameters
					.ToDictionary(x => x.OverriddenParameterName, x => x, StringComparer.OrdinalIgnoreCase);
			});
			_Setter = ReflectionUtils.CreateDelegate(CreateSetterDelegate,
				"setter delegate");
			_Command = new Lazy<IImmutableCommand>(() =>
			{
				var names = new[] { new Name(new[] { "__NamedTypeReaderSetter" }) };
				var command = new DelegateCommand(Setter, names);
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
			var instance = CreateInstance.Invoke();
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
				var value = trResult.Value!;

				var parameterInfo = new ParameterInfo(Command, parameter);
				foreach (var precondition in parameter.Preconditions)
				{
					var pResult = await precondition.CheckAsync(parameterInfo, context, value).ConfigureAwait(false);
					if (!pResult.IsSuccess)
					{
						// TODO: return a more descriptive error based on the failed precondition
						return TypeReaderResult<T>.Failure.Sync;
					}
				}

				Setter.Invoke(instance, parameter.ParameterName, value);
			}
			return TypeReaderResult<T>.FromSuccess(instance);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.NamedArguments
{
	public class NamedArgumentTypeReader<T> : TypeReader<T> where T : new()
	{
		private static readonly ITask<ITypeReaderResult<T>> _ArgCountError
			= TypeReaderResult<T>.FromError(NamedArgBadCountResult.Instance.Sync).AsITask();
		private static readonly char[] _TrimEndChars = new[] { ':' };
		private static readonly char[] _TrimStartChars = new[] { '/', '-' };

		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;
		private readonly Lazy<Action<T, string, object?>> _Setter;

		protected virtual IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;

		public NamedArgumentTypeReader()
		{
			_Parameters = new Lazy<IReadOnlyDictionary<string, IImmutableParameter>>(() =>
			{
				return NamedArgumentUtils
					.CreateParametersForType(typeof(T))
					.ToDictionary(x => x.ParameterName, StringComparer.OrdinalIgnoreCase);
			});
			_Setter = ReflectionUtils.CreateDelegate(CreateSetterDelegate,
				"setter delegate");
		}

		public override ITask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (input.Length % 2 != 0)
			{
				return _ArgCountError;
			}

			var result = TryCreateDict(input.Span, out var dict);
			if (!result.IsSuccess)
			{
				return TypeReaderResult<T>.FromError(result).AsITask();
			}

			return ReadDictIntoInstanceAsync(context, dict!);
		}

		protected virtual void Setter(T instance, string property, object? value)
			=> _Setter.Value.Invoke(instance, property, value);

		protected virtual IResult TryCreateDict(
			ReadOnlySpan<string> input,
			out IDictionary<string, string> dict)
		{
			dict = new Dictionary<string, string>();
			for (var i = 0; i < input.Length; i += 2)
			{
				var name = input[i].TrimStart(_TrimStartChars).TrimEnd(_TrimEndChars);
				if (!Parameters.TryGetValue(name, out var parameter))
				{
					return new NamedArgNonExistentResult(name);
				}

				var key = parameter.ParameterName;
				if (dict.ContainsKey(key))
				{
					return new NamedArgDuplicateResult(key);
				}
				dict.Add(key, input[i + 1]);
			}
			return SuccessResult.Instance.Sync;
		}

		private static Action<T, string, object?> CreateSetterDelegate()
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

			var instance = Expression.Parameter(typeof(T), "Instance");
			var name = Expression.Parameter(typeof(string), "Name");
			var value = Expression.Parameter(typeof(object), "Value");
			var returnLabel = Expression.Label();

			Expression CreateExpression(MemberInfo memberInfo, Type memberType)
			{
				// If Name == memberInfo.Name
				var memberName = Expression.Constant(memberInfo.Name);
				var isMember = Expression.Equal(memberName, name);

				// Then set member and return
				var member = Expression.MakeMemberAccess(instance, memberInfo);
				var type = Expression.Constant(memberType);
				var valueCast = Expression.Convert(value, memberType);
				var assign = Expression.Assign(member, valueCast);
				var @return = Expression.Return(returnLabel);
				var body = Expression.Block(assign, @return);

				return Expression.IfThen(isMember, body);
			}

			var (properties, fields) = typeof(T).GetWritableMembers();
			var assignProperties = properties.Select(x =>
			{
				return CreateExpression(x, x.PropertyType);
			});
			var assignFields = fields.Select(x =>
			{
				return CreateExpression(x, x.FieldType);
			});
			var body = Expression.Block(
				assignProperties
				.Concat(assignFields)
				.Append(Expression.Label(returnLabel))
			);

			var lambda = Expression.Lambda<Action<T, string, object?>>(
				body,
				instance,
				name,
				value
			);
			return lambda.Compile();
		}

		private async ITask<ITypeReaderResult<T>> ReadDictIntoInstanceAsync(
			IContext context,
			IDictionary<string, string> dict)
		{
			var registry = context.Services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();

			var instance = new T();
			foreach (var kvp in dict)
			{
				var args = kvp.Value;
				var parameter = Parameters[kvp.Key];
				var reader = registry.GetTypeReader(parameter);

				var result = await reader.ReadAsync(context, args).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess)
				{
					return TypeReaderResult<T>.FromError(result.InnerResult);
				}

				Setter(instance, parameter.OriginalParameterName, result.Value);
			}
			return TypeReaderResult<T>.FromSuccess(instance);
		}
	}
}
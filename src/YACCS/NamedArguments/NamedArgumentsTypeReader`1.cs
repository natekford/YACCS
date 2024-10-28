using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments;

/// <summary>
/// Parses a <typeparamref name="T"/> via named properties or arguments.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class NamedArgumentsTypeReader<T>
	: NamedArgumentsTypeReaderBase<T> where T : new()
{
	private readonly Action<T, string, object?> _Setter;

	/// <inheritdoc />
	protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }
		= typeof(T).CreateParamDict(x => x.ParameterName);

	/// <summary>
	/// Creates an instance of <see cref="NamedArgumentsTypeReader{T}"/>.
	/// </summary>
	public NamedArgumentsTypeReader()
	{
		_Setter = ReflectionUtils.CreateDelegate(Setter, "setter");
	}

	/// <inheritdoc />
	protected override void SetProperty(T instance, string property, object? value)
		=> _Setter.Invoke(instance, property, value);

	private static Action<T, string, object?> Setter()
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
		var setters = typeof(T).SelectWritableMembers(instance, access =>
		{
			// If Name == memberInfo.Name
			var memberName = Expression.Constant(access.Member.Name);
			var isMember = Expression.Equal(memberName, name);

			// Then set member and return
			var valueCast = Expression.Convert(value, access.Type);
			var assign = Expression.Assign(access, valueCast);
			var @return = Expression.Return(returnLabel);
			var body = Expression.Block(assign, @return);

			return Expression.IfThen(isMember, body);
		});
		var @return = Expression.Label(returnLabel);
		var body = Expression.Block(setters.Append(@return));

		var lambda = Expression.Lambda<Action<T, string, object?>>(
			body,
			instance,
			name,
			value
		);
		return lambda.Compile();
	}
}
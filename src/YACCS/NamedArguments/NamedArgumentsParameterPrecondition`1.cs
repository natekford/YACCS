﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.NamedArguments;

/// <summary>
/// Validates every property for the instance of <typeparamref name="T"/> passed in.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class NamedArgumentsParameterPrecondition<T>
	: NamedArgumentsParameterPreconditionBase<T>
{
	private static readonly object NotFound = new();
	private readonly Func<T, string, object> _Getter = ReflectionUtils.CreateDelegate(Getter);

	/// <inheritdoc />
	public override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }
		= typeof(T).CreateParamDict(x => x.OriginalParameterName);

	/// <inheritdoc />
	public override ValueTask<string> GetSummaryAsync(IContext context, IFormatProvider? formatProvider = null)
		=> this.CombineSummariesAsync(context);

	/// <inheritdoc />
	protected override bool TryGetProperty(T instance, string property, out object? value)
		=> (value = _Getter.Invoke(instance, property)) != NotFound;

	private static Func<T, string, object> Getter()
	{
		/*
		 *	(T Instance, string Name) =>
		 *	{
		 *		if (Name == "MemberA")
		 *		{
		 *			return Instance.MemberA;
		 *		}
		 *		if (Name == "MemberB")
		 *		{
		 *			return Instance.MemberB;
		 *		}
		 *		if (Name == "MemberC")
		 *		{
		 *			return Instance.MemberC;
		 *		}
		 *		return NotFound;
		 *	}
		 */

		var instance = Expression.Parameter(typeof(T), "Instance");
		var name = Expression.Parameter(typeof(string), "Name");

		var returnLabel = Expression.Label(typeof(object));
		var getters = typeof(T).SelectWritableMembers(instance, access =>
		{
			// If Name == memberInfo.Name
			var memberName = Expression.Constant(access.Member.Name);
			var isMember = Expression.Equal(memberName, name);

			// Then get member and return
			var cast = Expression.Convert(access, typeof(object));
			var @return = Expression.Return(returnLabel, cast);

			return Expression.IfThen(isMember, @return);
		});
		var notFound = Expression.Constant(NotFound);
		var @return = Expression.Label(returnLabel, notFound);
		var body = Expression.Block(getters.Append(@return));

		var lambda = Expression.Lambda<Func<T, string, object>>(
			body,
			instance,
			name
		);
		return lambda.Compile();
	}
}
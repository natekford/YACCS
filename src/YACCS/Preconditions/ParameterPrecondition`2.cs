using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions;

/// <summary>
/// The base class for a parameter precondition attribute.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TValue"></typeparam>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public abstract class ParameterPrecondition<TContext, TValue>
	: GroupablePrecondition, IParameterPrecondition<TContext, TValue>
	where TContext : IContext
{
	/// <inheritdoc />
	public abstract ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		TContext context,
		TValue? value);

	ValueTask<IResult> IParameterPrecondition.CheckAsync(
		CommandMeta meta,
		IContext context,
		object? value)
		=> CheckAsync(meta, context, value);

	/// <summary>
	/// Invokes <see cref="CheckAsync(CommandMeta, TContext, TValue?)"/> after handling
	/// type checking.
	/// </summary>
	/// <inheritdoc cref="CheckAsync(CommandMeta, TContext, TValue?)"/>
	protected virtual ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IContext context,
		object? value)
	{
		if (context is not TContext tContext)
		{
			return new(CachedResults.InvalidContext);
		}
		if (value is TValue tValue)
		{
			return CheckAsync(meta, tContext, tValue);
		}
		// If the value passed in is null, let CheckAsync deal with it
		if (value is null)
		{
			return CheckAsync(meta, tContext, default!);
		}
		// Not sure if this is the best way of dealing with IEnumerables
		//
		// The main issue with this is caching can't be used for each individual value
		// because I don't want to make the interfaces dependent upon PreconditionCache
		if (value is IEnumerable<TValue> tValues)
		{
			return CheckTypedEnumerableAsync(meta, tContext, tValues);
		}
		// Use the non generic interface to handle non nullable arrays
		// passed to nullable preconditions
		//
		// We can't rely solely on the non generic interface though, because something like
		// 'null is int?' returns false
		if (value is IEnumerable tUntypedValues)
		{
			return CheckUntypedEnumerableAsync(meta, tContext, tUntypedValues);
		}
		return new(CachedResults.InvalidParameter);
	}

	private async ValueTask<IResult> CheckTypedEnumerableAsync(
		CommandMeta meta,
		TContext context,
		IEnumerable<TValue> values)
	{
		foreach (var value in values)
		{
			var result = await CheckAsync(meta, context, value).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}
		}
		return CachedResults.Success;
	}

	private async ValueTask<IResult> CheckUntypedEnumerableAsync(
		CommandMeta meta,
		TContext context,
		IEnumerable values)
	{
		foreach (var value in values)
		{
			var tValue = value is TValue temp ? temp : default;
			if (value is not null && tValue is null)
			{
				return CachedResults.InvalidParameter;
			}

			var result = await CheckAsync(meta, context, tValue).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}
		}
		return CachedResults.Success;
	}
}
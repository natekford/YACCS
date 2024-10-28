using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Linq;

/// <inheritdoc />
public interface IParameter<in TValue> : IMutableParameter;

/// <summary>
/// Static methods for querying and modifying <see cref="IMutableParameter"/>.
/// </summary>
public static class Parameters
{
	/// <summary>
	/// Adds a parameter precondition to <paramref name="parameter"/>.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TParameter"></typeparam>
	/// <param name="parameter">The parameter to modify.</param>
	/// <param name="precondition">The precondition to add.</param>
	/// <returns><paramref name="parameter"/> after it has been modified.</returns>
	public static TParameter AddParameterPrecondition<TContext, TValue, TParameter>(
		this TParameter parameter,
		IParameterPrecondition<TContext, TValue> precondition)
		where TContext : IContext
		where TParameter : IMutableParameter, IParameter<TValue>
	{
		parameter.Attributes.Add(precondition);
		return parameter;
	}

	/// <summary>
	/// Casts <paramref name="entity"/> to <see cref="IMutableParameter"/>.
	/// </summary>
	/// <param name="entity">The entity to cast.</param>
	/// <returns><paramref name="entity"/> cast to <see cref="IMutableParameter"/>.</returns>
	/// <exception cref="ArgumentNullException">
	/// When <paramref name="entity"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// When <paramref name="entity"/> does not implement <see cref="IMutableParameter"/>.
	/// </exception>
	public static IMutableParameter AsParameter(this IQueryableEntity entity)
	{
		if (entity is null)
		{
			throw new ArgumentNullException(nameof(entity));
		}
		if (entity is not IMutableParameter parameter)
		{
			throw new ArgumentException($"Not a {nameof(IMutableParameter)}.", nameof(entity));
		}
		return parameter;
	}

	/// <summary>
	/// Converts <paramref name="parameter"/> to a generic version.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="parameter">The paramter to convert.</param>
	/// <returns>The generic version of <paramref name="parameter"/>.</returns>
	/// <exception cref="ArgumentException">
	/// When <typeparamref name="TValue"/> is invalid for <paramref name="parameter"/>.
	/// </exception>
	public static IParameter<TValue> AsType<TValue>(this IMutableParameter parameter)
	{
		if (!parameter.IsValidParameter(typeof(TValue)))
		{
			throw new ArgumentException(
				$"{typeof(TValue).FullName} is not and does not inherit or implement {parameter.ParameterType.Name}.", nameof(parameter));
		}
		return new Parameter<TValue>(parameter);
	}

	/// <summary>
	/// Creates a new <see cref="IParameter{TValue}"/>.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="name">The name to give this parameter.</param>
	/// <returns>A new parameter.</returns>
	public static IParameter<TValue> Create<TValue>(string name)
		=> new Parameter(typeof(TValue), name, null).AsType<TValue>();

	/// <summary>
	/// Filters <paramref name="parameters"/> by determining if <typeparamref name="TValue"/>
	/// is valid for each one.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="parameters">The parameters to filter.</param>
	/// <returns>An enumerable of paramters where <typeparamref name="TValue"/> is valid.</returns>
	public static IEnumerable<IParameter<TValue>> GetParametersByType<TValue>(
		this IEnumerable<IMutableParameter> parameters)
	{
		foreach (var parameter in parameters)
		{
			if (parameter.IsValidParameter(typeof(TValue)))
			{
				yield return new Parameter<TValue>(parameter);
			}
		}
	}

	/// <summary>
	/// Determines if <paramref name="type"/> is valid for <paramref name="parameter"/>.
	/// </summary>
	/// <param name="parameter">The parameter to check.</param>
	/// <param name="type">The type to check with.</param>
	/// <returns>A bool indicating success or failure.</returns>
	public static bool IsValidParameter(this IQueryableParameter parameter, Type type)
		=> parameter.ParameterType.IsAssignableFrom(type);

	/// <summary>
	/// Marks <paramref name="parameter"/> as a remainder if it currently does not have a
	/// <see cref="ILengthAttribute"/>.
	/// </summary>
	/// <typeparam name="TParameter"></typeparam>
	/// <param name="parameter">The parameter to modify.</param>
	/// <returns><paramref name="parameter"/> after it has been modified.</returns>
	public static TParameter MarkAsRemainder<TParameter>(this TParameter parameter)
		where TParameter : IMutableParameter
	{
		if (!parameter.GetAttributes<ILengthAttribute>().Any())
		{
			parameter.Attributes.Add(new RemainderAttribute());
		}
		return parameter;
	}

	/// <summary>
	/// Removes the default value from <paramref name="parameter"/>.
	/// </summary>
	/// <typeparam name="TParameter"></typeparam>
	/// <param name="parameter">The parameter to modify.</param>
	/// <returns><paramref name="parameter"/> after it has been modified.</returns>
	public static TParameter RemoveDefaultValue<TParameter>(this TParameter parameter)
		where TParameter : IMutableParameter
	{
		parameter.HasDefaultValue = false;
		return parameter;
	}

	/// <summary>
	/// Removes the type reader from <paramref name="parameter"/>.
	/// </summary>
	/// <typeparam name="TParameter"></typeparam>
	/// <param name="parameter">The parameter to modify.</param>
	/// <returns><paramref name="parameter"/> after it has been modified.</returns>
	public static TParameter RemoveTypeReader<TParameter>(this TParameter parameter)
		where TParameter : IMutableParameter
	{
		parameter.TypeReader = null;
		return parameter;
	}

	/// <summary>
	/// Set the default value for <paramref name="parameter"/> to <paramref name="value"/>.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TParameter"></typeparam>
	/// <param name="parameter">The parameter to modify.</param>
	/// <param name="value">The value to set.</param>
	/// <returns><paramref name="parameter"/> after it has been modified.</returns>
	public static TParameter SetDefaultValue<TValue, TParameter>(
		this TParameter parameter,
		TValue value)
		where TParameter : IMutableParameter, IParameter<TValue>
	{
		parameter.DefaultValue = value;
		return parameter;
	}

	/// <summary>
	/// Set the type reader for <paramref name="parameter"/> to <paramref name="typeReader"/>.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TParameter"></typeparam>
	/// <param name="parameter">The parameter to modify.</param>
	/// <param name="typeReader">The type reader to set.</param>
	/// <returns><paramref name="parameter"/> after it has been modified.</returns>
	public static TParameter SetTypeReader<TValue, TParameter>(
		this TParameter parameter,
		ITypeReader<TValue>? typeReader)
		where TParameter : IMutableParameter, IParameter<TValue>
	{
		parameter.TypeReader = typeReader;
		return parameter;
	}

	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	private sealed class Parameter<TValue>(IMutableParameter actual) : IParameter<TValue>
	{
		IList<object> IMutableEntity.Attributes
		{
			get => actual.Attributes;
			set => actual.Attributes = value;
		}
		IEnumerable<object> IQueryableEntity.Attributes => actual.Attributes;
		object? IMutableParameter.DefaultValue
		{
			get => actual.DefaultValue;
			set => actual.DefaultValue = value;
		}
		bool IMutableParameter.HasDefaultValue
		{
			get => actual.HasDefaultValue;
			set => actual.HasDefaultValue = value;
		}
		string IQueryableParameter.OriginalParameterName => actual.OriginalParameterName;
		Type IQueryableParameter.ParameterType => actual.ParameterType;
		ITypeReader? IMutableParameter.TypeReader
		{
			get => actual.TypeReader;
			set => actual.TypeReader = value;
		}
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		IImmutableParameter IMutableParameter.ToImmutable()
			=> actual.ToImmutable();
	}
}
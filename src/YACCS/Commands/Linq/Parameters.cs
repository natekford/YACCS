﻿using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Linq
{
	public interface IParameter<in TValue> : IParameter
	{
	}

	public static class Parameters
	{
		public static TParameter AddParameterPrecondition<TValue, TParameter>(
			this TParameter parameter,
			IParameterPrecondition<TValue> precondition)
			where TParameter : IParameter, IParameter<TValue>
		{
			parameter.Attributes.Add(precondition);
			return parameter;
		}

		public static IParameter AsParameter(this IQueryableEntity entity)
		{
			if (entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}
			if (entity is not IParameter parameter)
			{
				throw new ArgumentException("Not a parameter.", nameof(entity));
			}
			return parameter;
		}

		public static IParameter<TValue> AsType<TValue>(this IParameter parameter)
		{
			if (!parameter.IsValidParameter(typeof(TValue)))
			{
				throw new ArgumentException($"Is not and does not inherit or implement {parameter.ParameterType.Name}.", nameof(parameter));
			}
			return new Parameter<TValue>(parameter);
		}

		public static IParameter<TValue> GetParameterById<TValue>(
			this IEnumerable<IParameter> parameters,
			string id)
		{
			return parameters
				.ById(id)
				.Single()
				.AsType<TValue>();
		}

		public static IEnumerable<IParameter<TValue>> GetParametersById<TValue>(
			this IEnumerable<IParameter> parameters,
			string id)
		{
			return parameters
				.ById(id)
				.Select(AsType<TValue>);
		}

		public static IEnumerable<IParameter<TValue>> GetParametersByType<TValue>(
			this IEnumerable<IParameter> parameters)
		{
			foreach (var parameter in parameters)
			{
				if (parameter.IsValidParameter(typeof(TValue)))
				{
					yield return new Parameter<TValue>(parameter);
				}
			}
		}

		public static bool IsValidParameter(this IQueryableParameter parameter, Type type)
			=> parameter.ParameterType.IsAssignableFrom(type);

		public static TParameter RemoveDefaultValue<TParameter>(this TParameter parameter)
			where TParameter : IParameter
		{
			parameter.HasDefaultValue = false;
			return parameter;
		}

		public static TParameter RemoveTypeReader<TParameter>(
			this TParameter parameter)
			where TParameter : IParameter
		{
			parameter.TypeReader = null;
			return parameter;
		}

		public static TParameter SetDefaultValue<TValue, TParameter>(
			this TParameter parameter,
			TValue value)
			where TParameter : IParameter, IParameter<TValue>
		{
			parameter.DefaultValue = value;
			return parameter;
		}

		public static TParameter SetTypeReader<TValue, TParameter>(
			this TParameter parameter,
			ITypeReader<TValue>? typeReader)
			where TParameter : IParameter, IParameter<TValue>
		{
			parameter.TypeReader = typeReader;
			return parameter;
		}

		private sealed class Parameter<TValue> : IParameter<TValue>
		{
			private readonly IParameter _Actual;

			public IList<object> Attributes
			{
				get => _Actual.Attributes;
				set => _Actual.Attributes = value;
			}
			public object? DefaultValue
			{
				get => _Actual.DefaultValue;
				set => _Actual.DefaultValue = value;
			}
			public bool HasDefaultValue
			{
				get => _Actual.HasDefaultValue;
				set => _Actual.HasDefaultValue = value;
			}
			public ITypeReader<TValue>? OverridenTypeReader
			{
				get => _Actual.TypeReader as ITypeReader<TValue>;
				set => _Actual.TypeReader = value;
			}
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			public string ParameterName => _Actual.ParameterName;
			public Type ParameterType => _Actual.ParameterType;
			ITypeReader? IParameter.TypeReader
			{
				get => OverridenTypeReader;
				set => OverridenTypeReader = value as ITypeReader<TValue>;
			}

			public Parameter(IParameter actual)
			{
				_Actual = actual;
			}

			public IImmutableParameter ToImmutable()
				=> _Actual.ToImmutable();
		}
	}
}
using System;
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
		public static IParameter<TValue> AddParameterPrecondition<TValue, TPrecon>(
			this IParameter<TValue> parameter,
			TPrecon precondition)
			where TPrecon : IParameterPrecondition<TValue>
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
			if (!(entity is IParameter parameter))
			{
				throw new ArgumentException("Not a parameter.", nameof(entity));
			}
			return parameter;
		}

		public static IParameter<TValue> AsType<TValue>(this IParameter parameter)
		{
			if (!typeof(TValue).IsAssignableFrom(parameter.ParameterType))
			{
				throw new ArgumentException($"Is not and does not inherit or implement {parameter.ParameterType.Name}..", nameof(parameter));
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
				if (parameter.ParameterType.IsAssignableFrom(typeof(TValue)))
				{
					yield return new Parameter<TValue>(parameter);
				}
			}
		}

		public static IParameter<TValue> RemoveDefaultValue<TValue>(this IParameter<TValue> parameter)
		{
			// Where there is no default value, ParameterInfo.DefaultValue is a DBNull instance
			parameter.DefaultValue = DBNull.Value;
			return parameter;
		}

		public static IParameter<TValue> SetDefaultValue<TValue>(
			this IParameter<TValue> parameter,
			TValue value)
		{
			parameter.DefaultValue = value;
			return parameter;
		}

		public static IParameter<TValue> SetOverridenTypeReader<TValue>(
			this IParameter<TValue> parameter,
			ITypeReader<TValue> typeReader)
		{
			parameter.OverriddenTypeReader = typeReader;
			return parameter;
		}
	}

	public sealed class Parameter<TValue> : IParameter<TValue>
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
		public ITypeReader<TValue>? OverridenTypeReader
		{
			get => _Actual.OverriddenTypeReader as ITypeReader<TValue>;
			set => _Actual.OverriddenTypeReader = value;
		}
		public string ParameterName
		{
			get => _Actual.ParameterName;
			set => _Actual.ParameterName = value;
		}
		public Type ParameterType
		{
			get => _Actual.ParameterType;
			set => _Actual.ParameterType = value;
		}
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		ITypeReader? IParameter.OverriddenTypeReader
		{
			get => OverridenTypeReader;
			set => OverridenTypeReader = value as ITypeReader<TValue>;
		}

		public Parameter(IParameter actual)
		{
			_Actual = actual;
		}

		public IImmutableParameter ToParameter()
			=> _Actual.ToParameter();
	}
}
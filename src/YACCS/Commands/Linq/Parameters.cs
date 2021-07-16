using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Preconditions;
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
				throw new ArgumentException(
					$"{typeof(TValue).FullName} is not and does not inherit or implement {parameter.ParameterType.Name}.", nameof(parameter));
			}
			return new Parameter<TValue>(parameter);
		}

		public static IParameter<T> Create<T>(string name)
			=> new Parameter(typeof(T), name, null).AsType<T>();

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

		public static T MarkAsRemainder<T>(this T parameter) where T : IParameter
		{
			if (!parameter.Get<ILengthAttribute>().Any())
			{
				parameter.Attributes.Add(new RemainderAttribute());
			}
			return parameter;
		}

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

		[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
		private sealed class Parameter<TValue> : IParameter<TValue>
		{
			private readonly IParameter _Actual;

			IList<object> IEntityBase.Attributes
			{
				get => _Actual.Attributes;
				set => _Actual.Attributes = value;
			}
			IEnumerable<object> IQueryableEntity.Attributes => _Actual.Attributes;
			object? IParameter.DefaultValue
			{
				get => _Actual.DefaultValue;
				set => _Actual.DefaultValue = value;
			}
			bool IParameter.HasDefaultValue
			{
				get => _Actual.HasDefaultValue;
				set => _Actual.HasDefaultValue = value;
			}
			string IQueryableParameter.OriginalParameterName => _Actual.OriginalParameterName;
			Type IQueryableParameter.ParameterType => _Actual.ParameterType;
			ITypeReader? IParameter.TypeReader
			{
				get => _Actual.TypeReader;
				set => _Actual.TypeReader = (ITypeReader<TValue>?)value;
			}
			private string DebuggerDisplay => this.FormatForDebuggerDisplay();

			public Parameter(IParameter actual)
			{
				_Actual = actual;
			}

			IImmutableParameter IParameter.ToImmutable()
				=> _Actual.ToImmutable();
		}
	}
}
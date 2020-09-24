using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class Parameter : EntityBase, IParameter
	{
		public object? DefaultValue { get; set; } = DBNull.Value;
		public ITypeReader? OverriddenTypeReader { get; set; }
		public string ParameterName { get; set; }
		public Type ParameterType { get; set; }
		private string DebuggerDisplay => $"Name = {ParameterName}, Type = {ParameterType}";

		public Parameter() : base(null)
		{
			ParameterName = "";
			ParameterType = typeof(void);
		}

		public Parameter(ParameterInfo parameter) : base(parameter)
		{
			DefaultValue = GetDefaultValue(parameter);
			ParameterName = parameter.Name;
			ParameterType = parameter.ParameterType;
		}

		public IImmutableParameter ToParameter()
			=> new ImmutableParameter(this);

		private static object? GetDefaultValue(ParameterInfo parameter)
		{
			// Not optional and has no default value
			if (parameter.DefaultValue == DBNull.Value)
			{
				return DBNull.Value;
			}
			// Optional but has no default value
			if (parameter.DefaultValue == Type.Missing)
			{
				return DBNull.Value;
			}
			return parameter.DefaultValue;
		}

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		private sealed class ImmutableParameter : IImmutableParameter
		{
			public IReadOnlyList<object> Attributes { get; }
			public object? DefaultValue { get; }
			public Type? EnumerableType { get; }
			public int Length { get; }
			public ITypeReader? OverriddenTypeReader { get; }
			public string ParameterName { get; }
			public Type ParameterType { get; }
			public IReadOnlyList<IParameterPrecondition> Preconditions { get; }
			public string PrimaryId { get; }
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			private string DebuggerDisplay => $"Name = {ParameterName}, Type = {ParameterType}";

			public ImmutableParameter(Parameter mutable)
			{
				if (mutable.ParameterType == typeof(void))
				{
					throw new ArgumentException("Cannot have a parameter type of void.", nameof(mutable));
				}

				Attributes = mutable.Attributes.ToImmutableArray();
				DefaultValue = mutable.DefaultValue;
				EnumerableType = GetEnumerableType(mutable.ParameterType);
				Length = mutable.Get<ILengthAttribute>().SingleOrDefault()?.Length ?? 1;
				OverriddenTypeReader = mutable.OverriddenTypeReader;
				ParameterName = mutable.ParameterName;
				ParameterType = mutable.ParameterType;
				Preconditions = mutable.Get<IParameterPrecondition>().ToImmutableArray();
				PrimaryId = mutable.Get<IIdAttribute>().FirstOrDefault()?.Id ?? Guid.NewGuid().ToString();
			}

			private static Type? GetEnumerableType(Type type)
			{
				if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					return type.GetGenericArguments()[0];
				}
				foreach (var @interface in type.GetInterfaces())
				{
					if (@interface.IsGenericType
						&& @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					{
						return @interface.GetGenericArguments()[0];
					}
				}
				return null;
			}
		}
	}
}
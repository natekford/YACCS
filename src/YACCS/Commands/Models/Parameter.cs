using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class Parameter : EntityBase, IParameter
	{
		private static readonly object NoDefaultValue = new object();

		private ITypeReader? _OverriddenTypeReader;

		public object? DefaultValue { get; set; } = NoDefaultValue;
		public bool HasDefaultValue
		{
			get => DefaultValue != NoDefaultValue;
			set
			{
				if (!value)
				{
					DefaultValue = NoDefaultValue;
				}
			}
		}
		public ITypeReader? OverriddenTypeReader
		{
			get => _OverriddenTypeReader;
			set
			{
				if (value is not null)
				{
					value.ThrowIfInvalidTypeReader(ParameterType);
				}
				_OverriddenTypeReader = value;
			}
		}
		public string ParameterName { get; }
		public Type ParameterType { get; }
		private string DebuggerDisplay => $"Name = {ParameterName}, Type = {ParameterType}";

		public Parameter() : base(null)
		{
			ParameterName = "";
			ParameterType = typeof(void);
		}

		public Parameter(Type type, string name) : base(null)
		{
			ParameterName = name;
			ParameterType = type;
		}

		public Parameter(FieldInfo field) : base(field)
		{
			ParameterName = field.Name;
			ParameterType = field.FieldType;
		}

		public Parameter(PropertyInfo property) : base(property)
		{
			ParameterName = property.Name;
			ParameterType = property.PropertyType;
		}

		public Parameter(System.Reflection.ParameterInfo parameter) : base(parameter)
		{
			DefaultValue = GetDefaultValue(parameter);
			ParameterName = parameter.Name;
			ParameterType = parameter.ParameterType;
		}

		public IImmutableParameter ToParameter()
			=> new ImmutableParameter(this);

		private static object? GetDefaultValue(System.Reflection.ParameterInfo parameter)
		{
			// Not optional and has no default value
			if (parameter.DefaultValue == DBNull.Value)
			{
				return NoDefaultValue;
			}
			// Optional but has no default value
			if (parameter.DefaultValue == Type.Missing)
			{
				return NoDefaultValue;
			}
			return parameter.DefaultValue;
		}

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		private sealed class ImmutableParameter : IImmutableParameter
		{
			// Some interfaces Array implements
			// Don't deal with the non generic versions b/c how would we parse 'object'?
			private static readonly Type[] _SupportedEnumerableTypes = new[]
			{
				typeof(IList<>),
				typeof(ICollection<>),
				typeof(IEnumerable<>),
				typeof(IReadOnlyList<>),
				typeof(IReadOnlyCollection<>),
			};

			public IReadOnlyList<object> Attributes { get; }
			public object? DefaultValue { get; }
			public Type? EnumerableType { get; }
			public bool HasDefaultValue { get; }
			public int? Length { get; }
			public string OverriddenParameterName { get; }
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
				HasDefaultValue = mutable.HasDefaultValue;
				var length = mutable.Get<ILengthAttribute>().SingleOrDefault();
				Length = length == null ? 1 : length.Length;
				// TODO: add in override type readers from attribute
				// TODO: add in override name from attribute
				OverriddenParameterName = mutable.ParameterName;
				OverriddenTypeReader = mutable.OverriddenTypeReader;
				ParameterName = mutable.ParameterName;
				ParameterType = mutable.ParameterType;
				Preconditions = mutable.Get<IParameterPrecondition>().ToImmutableArray();
				PrimaryId = mutable.Get<IIdAttribute>().FirstOrDefault()?.Id ?? Guid.NewGuid().ToString();
			}

			private static Type? GetEnumerableType(Type type)
			{
				if (type.IsArray)
				{
					return type.GetElementType();
				}
				if (type.IsInterface && type.IsGenericType)
				{
					var typeDefinition = type.GetGenericTypeDefinition();
					foreach (var supportedType in _SupportedEnumerableTypes)
					{
						if (typeDefinition == supportedType)
						{
							return type.GetGenericArguments()[0];
						}
					}
				}
				return null;
			}
		}
	}
}
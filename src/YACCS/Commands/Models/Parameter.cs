using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.NamedArguments;
using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	using ParameterInfo = System.Reflection.ParameterInfo;

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

		public Parameter() : this(typeof(void), "", null)
		{
		}

		public Parameter(Type type, string name, ICustomAttributeProvider? provider)
			: base(provider)
		{
			if (type == typeof(void))
			{
				throw new ArgumentException("Cannot have a parameter type of void.", nameof(type));
			}

			ParameterName = name;
			ParameterType = type;

			if (this.Get<GenerateNamedArgumentsAttribute>().Any()
				|| type.GetCustomAttribute<GenerateNamedArgumentsAttribute>() != null)
			{
				if (!this.Get<ICountAttribute>().Any())
				{
					Attributes.Add(new RemainderAttribute());
				}

				var ppType = typeof(NamedArgumentParameterPrecondition<>).MakeGenericType(ParameterType);
				Attributes.Add(Activator.CreateInstance(ppType));
			}
		}

		public Parameter(FieldInfo field)
			: this(field.FieldType, field.Name, field)
		{
		}

		public Parameter(PropertyInfo property)
			: this(property.PropertyType, property.Name, property)
		{
		}

		public Parameter(ParameterInfo parameter)
			: this(parameter.ParameterType, parameter.Name, parameter)
		{
			DefaultValue = GetDefaultValue(parameter);
		}

		public IImmutableParameter ToImmutable()
			=> new ImmutableParameter(this);

		private static object? GetDefaultValue(ParameterInfo parameter)
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
			public Type? ElementType { get; }
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
				Attributes = mutable.Attributes.ToImmutableArray();
				DefaultValue = mutable.DefaultValue;
				ElementType = GetEnumerableType(mutable.ParameterType);
				HasDefaultValue = mutable.HasDefaultValue;
				var length = mutable.Get<ICountAttribute>().SingleOrDefault();
				Length = length == null ? 1 : length.Count;
				OverriddenParameterName = mutable.Get<INameAttribute>().SingleOrDefault()?.Name
					?? mutable.ParameterName;
				// TODO: add in override type readers from attribute
				OverriddenTypeReader = mutable.OverriddenTypeReader;
				ParameterName = mutable.ParameterName;
				ParameterType = mutable.ParameterType;
				Preconditions = mutable.Get<IParameterPrecondition>().ToImmutableArray();
				PrimaryId = mutable.Get<IIdAttribute>().FirstOrDefault()?.Id
					?? Guid.NewGuid().ToString();
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
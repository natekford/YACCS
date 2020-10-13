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

		private readonly bool? _HasLengthAttribute;
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
		public string ParameterName { get; }
		public Type ParameterType { get; }
		public ITypeReader? TypeReader
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
				var ppType = typeof(NamedArgumentParameterPrecondition<>).MakeGenericType(ParameterType);
				Attributes.Add(Activator.CreateInstance(ppType));
				AddRemainderAttribute(ref _HasLengthAttribute);
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

			if (this.Get<ParamArrayAttribute>().Any())
			{
				AddRemainderAttribute(ref _HasLengthAttribute);
			}
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

		private void AddRemainderAttribute(ref bool? flag)
		{
			if (flag == true || this.Get<ILengthAttribute>().Any())
			{
				return;
			}

			flag = true;
			Attributes.Add(new RemainderAttribute());
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
			public int? Length { get; } = 1;
			public string OverriddenParameterName { get; }
			public string ParameterName { get; }
			public Type ParameterType { get; }
			public IReadOnlyList<IParameterPrecondition> Preconditions { get; }
			public string PrimaryId { get; }
			public ITypeReader? TypeReader { get; }
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			private string DebuggerDisplay => $"Name = {ParameterName}, Type = {ParameterType}";

			public ImmutableParameter(Parameter mutable)
			{
				DefaultValue = mutable.DefaultValue;
				ElementType = GetEnumerableType(mutable.ParameterType);
				HasDefaultValue = mutable.HasDefaultValue;
				ParameterName = mutable.ParameterName;
				ParameterType = mutable.ParameterType;
				// TODO: add in type readers from attribute
				TypeReader = mutable.TypeReader;

				{
					var attributes = ImmutableArray.CreateBuilder<object>(mutable.Attributes.Count);
					var preconditions = new List<IParameterPrecondition>();
					int l = 0, n = 0;
					foreach (var attribute in mutable.Attributes)
					{
						attributes.Add(attribute);
						switch (attribute)
						{
							case IParameterPrecondition precondition:
								preconditions.Add(precondition);
								break;

							case ILengthAttribute length:
								Length = length.ThrowIfDuplicate(x => x.Length, ref l);
								break;

							case INameAttribute name:
								OverriddenParameterName = name.ThrowIfDuplicate(x => x.Name, ref n);
								break;

							case IIdAttribute id:
								PrimaryId ??= id.Id;
								break;
						}
					}
					Attributes = attributes.MoveToImmutable();
					Preconditions = preconditions.ToImmutableArray();
				}

				OverriddenParameterName ??= mutable.ParameterName;
				PrimaryId ??= Guid.NewGuid().ToString();
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
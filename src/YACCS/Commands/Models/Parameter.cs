using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.NamedArguments;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class Parameter : EntityBase, IParameter
	{
		private static readonly object NoDefaultValue = new();

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
		public string OriginalParameterName { get; }
		public Type ParameterType { get; }
		public ITypeReader? TypeReader
		{
			get => _OverriddenTypeReader;
			set
			{
				value?.ThrowIfInvalidTypeReader(ParameterType);
				_OverriddenTypeReader = value;
			}
		}
		private string DebuggerDisplay => $"Name = {OriginalParameterName}, Type = {ParameterType}";

		public Parameter(Type type, string name, ICustomAttributeProvider? provider)
			: base(provider)
		{
			if (type == typeof(void))
			{
				throw new ArgumentException($"'{name}' cannot have a parameter type of void.", nameof(type));
			}

			OriginalParameterName = name;
			ParameterType = type;

			if (this.Get<GenerateNamedArgumentsAttribute>().Any()
				|| type.GetCustomAttribute<GenerateNamedArgumentsAttribute>() is not null)
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
			DefaultValue = GetDefaultValue(parameter.DefaultValue);

			if (this.Get<ParamArrayAttribute>().Any())
			{
				AddRemainderAttribute(ref _HasLengthAttribute);
			}
		}

		public IImmutableParameter ToImmutable()
			=> new ImmutableParameter(this);

		private static object? GetDefaultValue(object value)
		{
			// Not optional and has no default value
			if (value == DBNull.Value)
			{
				return NoDefaultValue;
			}
			// Optional but has no default value
			if (value == Type.Missing)
			{
				return NoDefaultValue;
			}
			return value;
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
			public IReadOnlyList<object> Attributes { get; }
			public object? DefaultValue { get; }
			public bool HasDefaultValue { get; }
			public int? Length { get; } = 1;
			public string OriginalParameterName { get; }
			public string ParameterName { get; }
			public Type ParameterType { get; }
			public IReadOnlyList<IParameterPrecondition> Preconditions { get; }
			public string PrimaryId { get; }
			public ITypeReader? TypeReader { get; }
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			private string DebuggerDisplay => $"Name = {OriginalParameterName}, Type = {ParameterType}";

			public ImmutableParameter(Parameter mutable)
			{
				DefaultValue = mutable.DefaultValue;
				HasDefaultValue = mutable.HasDefaultValue;
				OriginalParameterName = mutable.OriginalParameterName;
				ParameterType = mutable.ParameterType;

				{
					var builder = ImmutableArray.CreateBuilder<object>(mutable.Attributes.Count);
					var preconditions = new List<IParameterPrecondition>();
					int l = 0, n = 0, t = 0;
					foreach (var attribute in mutable.Attributes)
					{
						builder.Add(attribute);
						switch (attribute)
						{
							case IParameterPrecondition precondition:
								preconditions.Add(precondition);
								break;

							case ILengthAttribute length:
								Length = length.ThrowIfDuplicate(x => x.Length, ref l);
								break;

							case INameAttribute name:
								ParameterName = name.ThrowIfDuplicate(x => x.Name, ref n);
								break;

							case IOverrideTypeReaderAttribute typeReader:
								typeReader.Reader.ThrowIfInvalidTypeReader(ParameterType);
								TypeReader = typeReader.ThrowIfDuplicate(x => x.Reader, ref t);
								break;

							case IIdAttribute id:
								PrimaryId ??= id.Id;
								break;
						}
					}
					Attributes = builder.MoveToImmutable();
					Preconditions = preconditions.ToImmutableArray();
				}

				TypeReader ??= mutable.TypeReader;
				ParameterName ??= mutable.OriginalParameterName;
				PrimaryId ??= Guid.NewGuid().ToString();
			}
		}
	}
}
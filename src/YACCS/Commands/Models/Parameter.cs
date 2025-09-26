using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Linq;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models;

/// <inheritdoc cref="IMutableParameter"/>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public sealed class Parameter : Entity, IMutableParameter
{
	private static readonly object NoDefaultValue = new();

	private ITypeReader? _TypeReader;

	/// <inheritdoc />
	public object? DefaultValue { get; set; } = NoDefaultValue;
	/// <inheritdoc />
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
	/// <inheritdoc />
	public string OriginalParameterName { get; }
	/// <inheritdoc />
	public Type ParameterType { get; }
	/// <inheritdoc />
	public ITypeReader? TypeReader
	{
		get => _TypeReader;
		set
		{
			value?.ThrowIfInvalidTypeReader(ParameterType);
			_TypeReader = value;
		}
	}
	private string DebuggerDisplay => this.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates a new <see cref="Parameter"/>.
	/// </summary>
	/// <param name="type">The type of this parameter.</param>
	/// <param name="name">The original name of this parameter.</param>
	/// <param name="provider">The object providing custom attributes.</param>
	public Parameter(Type type, string name, ICustomAttributeProvider? provider)
		: base(provider)
	{
		if (type == typeof(void))
		{
			throw new ArgumentException($"'{name}' cannot have a parameter type of void.", nameof(type));
		}

		OriginalParameterName = name;
		ParameterType = type;

		// After everything else has been set, some attributes may add additional stuff
		// like the named arguments attribute on the parameter's type will add
		// the named arguments parameter precondition which verifies that every
		// property has been set or has a default value
		var parameterTypeAttributes = type.GetCustomAttributes(true);
		var parameterAttributes = Attributes.Select(x => x.Value);
		foreach (var attribute in parameterAttributes.Concat(parameterTypeAttributes))
		{
			if (attribute is IParameterModifierAttribute modifier)
			{
				modifier.ModifyParameter(this);
			}
		}
	}

	/// <inheritdoc cref="Parameter(Type, string, ICustomAttributeProvider?)"/>
	/// <param name="field">The field to use as a parameter.</param>
	public Parameter(FieldInfo field)
		: this(field.FieldType, field.Name, field)
	{
	}

	/// <inheritdoc cref="Parameter(Type, string, ICustomAttributeProvider?)"/>
	/// <param name="property">The property to use as a parameter.</param>
	public Parameter(PropertyInfo property)
		: this(property.PropertyType, property.Name, property)
	{
	}

	/// <inheritdoc cref="Parameter(Type, string, ICustomAttributeProvider?)"/>
	/// <param name="parameter">The parameter to use as a parameter.</param>
	public Parameter(ParameterInfo parameter)
		: this(parameter.ParameterType, parameter.Name, parameter)
	{
		DefaultValue = GetDefaultValue(parameter.DefaultValue);

		if (this.GetAttributes<ParamArrayAttribute>().Any())
		{
			this.MarkAsRemainder();
		}
	}

	/// <inheritdoc />
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

	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	private sealed class ImmutableParameter : IImmutableParameter
	{
		private readonly Func<object?>? _GetDefaultValue;

		public IReadOnlyList<AttributeInfo> Attributes { get; }
		public object? DefaultValue => _GetDefaultValue?.Invoke();
		public bool HasDefaultValue { get; }
		public int? Length { get; } = 1;
		public string OriginalParameterName { get; }
		public INameAttribute? ParameterName { get; }
		public Type ParameterType { get; }
		public IReadOnlyDictionary<string, IReadOnlyList<IParameterPrecondition>> Preconditions { get; }
		public string PrimaryId { get; }
		public ITypeReader? TypeReader { get; }
		IEnumerable<AttributeInfo> IQueryableEntity.Attributes => Attributes;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		public ImmutableParameter(Parameter mutable)
		{
			HasDefaultValue = mutable.HasDefaultValue;
			OriginalParameterName = mutable.OriginalParameterName;
			ParameterType = mutable.ParameterType;

			if (mutable.HasDefaultValue)
			{
				if (mutable.DefaultValue is null && mutable.ParameterType.IsValueType)
				{
					_GetDefaultValue = GenerateDefault(mutable.ParameterType);
				}
				else
				{
					var defaultValue = mutable.DefaultValue;
					_GetDefaultValue = () => defaultValue;
				}
			}

			var attributes = ImmutableArray.CreateBuilder<AttributeInfo>(mutable.Attributes.Count);
			// Use ConcurrentDictionary because it has GetOrAdd by default, not threading reasons
			var preconditions = new ConcurrentDictionary<string, List<IParameterPrecondition>>();
			int l = 0, n = 0, t = 0;
			foreach (var attribute in mutable.Attributes)
			{
				attributes.Add(attribute);
				// No if/else in case some madman decides to implement multiple
				if (attribute.Value is IParameterPrecondition precondition)
				{
					preconditions.AddPrecondition(precondition);
				}
				if (attribute.Value is ILengthAttribute length)
				{
					Length = length.ThrowIfDuplicate(x => x.Length, ref l);
				}
				if (attribute.Value is INameAttribute name)
				{
					ParameterName = name.ThrowIfDuplicate(x => x, ref n);
				}
				if (attribute.Value is IOverrideTypeReaderAttribute typeReader)
				{
					typeReader.Reader.ThrowIfInvalidTypeReader(ParameterType);
					TypeReader = typeReader.ThrowIfDuplicate(x => x.Reader, ref t);
				}
				if (attribute.Value is IIdAttribute id)
				{
					PrimaryId ??= id.Id;
				}
			}
			Attributes = attributes.MoveToImmutable();
			Preconditions = preconditions.ToImmutablePreconditions();

			TypeReader ??= mutable.TypeReader;
			PrimaryId ??= Guid.NewGuid().ToString();
		}

		private static Func<object?> GenerateDefault(Type type)
		{
			var body = Expression.Convert(Expression.Default(type), typeof(object));

			var lambda = Expression.Lambda<Func<object>>(body);
			return lambda.Compile();
		}
	}
}
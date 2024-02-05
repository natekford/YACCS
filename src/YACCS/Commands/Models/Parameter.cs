using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
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

	private ITypeReader? _OverriddenTypeReader;

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
		get => _OverriddenTypeReader;
		set
		{
			value?.ThrowIfInvalidTypeReader(ParameterType);
			_OverriddenTypeReader = value;
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
		foreach (var attribute in type.GetCustomAttributes(true).Concat(BaseAttributes))
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
		public IReadOnlyList<object> Attributes { get; }
		public object? DefaultValue { get; }
		public bool HasDefaultValue { get; }
		public int? Length { get; } = 1;
		public string OriginalParameterName { get; }
		public string ParameterName { get; }
		public Type ParameterType { get; }
		public IReadOnlyDictionary<string, IReadOnlyList<IParameterPrecondition>> Preconditions { get; }
		public string PrimaryId { get; }
		public ITypeReader? TypeReader { get; }
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		public ImmutableParameter(Parameter mutable)
		{
			DefaultValue = mutable.DefaultValue;
			HasDefaultValue = mutable.HasDefaultValue;
			OriginalParameterName = mutable.OriginalParameterName;
			ParameterType = mutable.ParameterType;

			var attributes = ImmutableArray.CreateBuilder<object>(mutable.Attributes.Count);
			// Use ConcurrentDictionary because it has GetOrAdd by default, not threading reasons
			var preconditions = new ConcurrentDictionary<string, List<IParameterPrecondition>>();
			int l = 0, n = 0, t = 0;
			foreach (var attribute in mutable.Attributes)
			{
				attributes.Add(attribute);
				// No if/else in case some madman decides to implement multiple
				if (attribute is IParameterPrecondition precondition)
				{
					preconditions.AddPrecondition(precondition);
				}
				if (attribute is ILengthAttribute length)
				{
					Length = length.ThrowIfDuplicate(x => x.Length, ref l);
				}
				if (attribute is INameAttribute name)
				{
					ParameterName = name.ThrowIfDuplicate(x => x.Name, ref n);
				}
				if (attribute is IOverrideTypeReaderAttribute typeReader)
				{
					typeReader.Reader.ThrowIfInvalidTypeReader(ParameterType);
					TypeReader = typeReader.ThrowIfDuplicate(x => x.Reader, ref t);
				}
				if (attribute is IIdAttribute id)
				{
					PrimaryId ??= id.Id;
				}
			}
			Attributes = attributes.MoveToImmutable();
			Preconditions = preconditions.ToImmutablePreconditions();

			TypeReader ??= mutable.TypeReader;
			ParameterName ??= mutable.OriginalParameterName;
			PrimaryId ??= Guid.NewGuid().ToString();
		}
	}
}
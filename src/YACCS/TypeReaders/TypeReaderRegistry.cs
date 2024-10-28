using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using YACCS.Commands;

namespace YACCS.TypeReaders;

/// <summary>
/// A registry for <see cref="ITypeReader"/>s.
/// </summary>
public class TypeReaderRegistry : TypeRegistry<ITypeReader>
{
	private static readonly MethodInfo _CreateAggregateTypeReader =
		typeof(TypeReaderRegistry)
		.GetTypeInfo()
		.DeclaredMethods
		.Single(x => x.Name == nameof(CreateAggregateTypeReader));
	private static readonly MethodInfo _RegisterStruct =
		typeof(TypeReaderRegistry)
		.GetTypeInfo()
		.DeclaredMethods
		.Single(x => x.Name == nameof(RegisterStruct));

	/// <inheritdoc />
	protected override IDictionary<Type, ITypeReader> Items { get; }
		= new Dictionary<Type, ITypeReader>();

	/// <summary>
	/// Creates a new <see cref="TypeReaderRegistry"/>.
	/// </summary>
	/// <param name="assemblies">The assemblies to scan for type readers.</param>
	public TypeReaderRegistry(IEnumerable<Assembly>? assemblies = null)
	{
		RegisterClass(new StringTypeReader());
		RegisterClass(new UriTypeReader());
		RegisterClass(new FileInfoTypeReader());
		RegisterClass(new DirectoryInfoTypeReader());
		RegisterStruct(new TryParseTypeReader<char>(char.TryParse));
		RegisterStruct(new TryParseTypeReader<bool>(bool.TryParse));
		RegisterStruct(new NumberTypeReader<sbyte>(sbyte.TryParse));
		RegisterStruct(new NumberTypeReader<byte>(byte.TryParse));
		RegisterStruct(new NumberTypeReader<short>(short.TryParse));
		RegisterStruct(new NumberTypeReader<ushort>(ushort.TryParse));
		RegisterStruct(new NumberTypeReader<int>(int.TryParse));
		RegisterStruct(new NumberTypeReader<uint>(uint.TryParse));
		RegisterStruct(new NumberTypeReader<long>(long.TryParse));
		RegisterStruct(new NumberTypeReader<ulong>(ulong.TryParse));
		RegisterStruct(new NumberTypeReader<float>(float.TryParse));
		RegisterStruct(new NumberTypeReader<double>(double.TryParse));
		RegisterStruct(new NumberTypeReader<decimal>(decimal.TryParse));
		RegisterStruct(new DateTimeTypeReader<DateTime>(DateTime.TryParse));
		RegisterStruct(new DateTimeTypeReader<DateTimeOffset>(DateTimeOffset.TryParse));
		RegisterStruct(new TimeSpanTypeReader<TimeSpan>(TimeSpan.TryParse));

		RegisterTypeReaders(typeof(TypeReaderRegistry).Assembly.GetCustomTypeReaders());
		if (assemblies is not null)
		{
			foreach (var assembly in assemblies)
			{
				RegisterTypeReaders(assembly.GetCustomTypeReaders());
			}
		}
	}

	/// <summary>
	/// Adds a type reader to this registry.
	/// </summary>
	/// <param name="type">The type the reader is for.</param>
	/// <param name="reader">The reader to add.</param>
	/// <exception cref="ArgumentException">
	/// When the output type of <paramref name="reader"/> cannot be assigned to
	/// a variable of <paramref name="type"/>.
	/// </exception>
	public void Register(Type type, ITypeReader reader)
	{
		reader.ThrowIfInvalidTypeReader(type);
		if (type.IsValueType)
		{
			_RegisterStruct.MakeGenericMethod(type).Invoke(this, new[] { reader });
		}
		else
		{
			Items[type] = reader;
		}
	}

	/// <summary>
	/// Adds a type reader to this registry.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="reader">The reader to add.</param>
	public void RegisterClass<T>(ITypeReader<T> reader) where T : class
		=> Items[typeof(T)] = reader;

	/// <summary>
	/// Adds a type reader and its nullable variant to this registry.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="reader">The reader to add.</param>
	public void RegisterStruct<T>(ITypeReader<T> reader) where T : struct
	{
		Items[typeof(T)] = reader;
		Items[typeof(T?)] = new NullableTypeReader<T>();
	}

	/// <inheritdoc />
	public override bool TryGetValue(Type type, [NotNullWhen(true)] out ITypeReader? reader)
	{
		if (Items.TryGetValue(type, out reader))
		{
			return true;
		}
		if (TryCreateReader(type, out reader))
		{
			Register(type, reader);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Adds the readers to this registry.
	/// </summary>
	/// <param name="typeReaders">The readers to add.</param>
	/// <exception cref="InvalidOperationException">
	/// When a reader has already been registered for a type and
	/// <see cref="TypeReaderInfo.OverrideExistingTypeReaders"/> is false.
	/// </exception>
	protected void RegisterTypeReaders(IEnumerable<TypeReaderInfo> typeReaders)
	{
		foreach (var typeReader in typeReaders)
		{
			foreach (var type in typeReader.TargetTypes)
			{
				if (typeReader.OverrideExistingTypeReaders || !Items.ContainsKey(type))
				{
					Register(type, typeReader.Instance);
					continue;
				}

				throw new InvalidOperationException("Attempted to register " +
					$"{typeReader.Instance.GetType()} to {type} while that " +
					"type already has a reader registered. " +
					$"Set {nameof(TypeReaderTargetTypesAttribute.OverrideExistingTypeReaders)} " +
					$"to {true} to replace the registered type reader.");
			}
		}
	}

	/// <summary>
	/// Tries to create a <see cref="ITypeReader"/> for <paramref name="type"/>.
	/// </summary>
	/// <remarks>
	/// If <paramref name="type"/> has any attributes implementing
	/// <see cref="ITypeReaderGeneratorAttribute"/> those will be used. Otherwise,
	/// this supports <see cref="EnumTypeReader{TEnum}"/>,
	/// <see cref="ContextTypeReader{TContext}"/>, <see cref="ArrayTypeReader{T}"/>,
	/// <see cref="ListTypeReader{T}"/>, and <see cref="HashSetTypeReader{T}"/>.
	/// </remarks>
	/// <param name="type">The type to create a type reader for.</param>
	/// <param name="reader">The created type reader.</param>
	/// <returns>A bool indicating success or failure.</returns>
	protected virtual bool TryCreateReader(Type type, [NotNullWhen(true)] out ITypeReader? reader)
	{
		var customReaders = type
			.GetCustomAttributes()
			.OfType<ITypeReaderGeneratorAttribute>()
			.Select(x => x.GenerateTypeReader(type))
			.ToList();
		if (customReaders.Count == 1)
		{
			reader = customReaders[0];
			return true;
		}
		else if (customReaders.Count > 1)
		{
			var factory = _CreateAggregateTypeReader.MakeGenericMethod(type);
			reader = (ITypeReader)factory.Invoke(null, [customReaders]);
			return true;
		}

		Type readerType;
		if (type.IsEnum)
		{
			readerType = typeof(EnumTypeReader<>).MakeGenericType(type);
		}
		else if (typeof(IContext).IsAssignableFrom(type))
		{
			readerType = typeof(ContextTypeReader<>).MakeGenericType(type);
		}
		else if (type.TryGetCollectionType(out var cType) && Items.ContainsKey(cType))
		{
			var typeDef = type.IsArray ? typeof(ArrayTypeReader<>) : typeof(ListTypeReader<>);
			readerType = typeDef.MakeGenericType(cType);
		}
		else if (type.TryGetHashSetType(out var sType) && Items.ContainsKey(sType))
		{
			readerType = typeof(HashSetTypeReader<>).MakeGenericType(sType);
		}
		else
		{
			reader = null;
			return false;
		}

		reader = readerType.CreateInstance<ITypeReader>();
		return true;
	}

	private static AggregateTypeReader<T> CreateAggregateTypeReader<T>(
		IEnumerable<ITypeReader> typeReaders)
		=> new(typeReaders.Cast<ITypeReader<T>>());
}
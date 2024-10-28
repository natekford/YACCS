using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using YACCS.Localization;
using YACCS.TypeReaders;

using LKeys = YACCS.Localization.Keys;

namespace YACCS.Help;

/// <summary>
/// A registry for outputting the names of <see cref="Type"/>s to the end user.
/// </summary>
public class TypeNameRegistry : TypeRegistry<string>
{
	/// <inheritdoc />
	protected override IDictionary<Type, string> Items => Localized.GetCurrent();
	/// <summary>
	/// The localized instance backing <see cref="Items"/>.
	/// </summary>
	protected virtual Localized<IDictionary<Type, string>> Localized { get; }

	/// <summary>
	/// Creates a new <see cref="TypeNameRegistry"/>.
	/// </summary>
	public TypeNameRegistry()
	{
		Localized = new(_ =>
		{
			return new Dictionary<Type, string>()
			{
				[typeof(string)] = LKeys.StringType.Localized,
				[typeof(Uri)] = LKeys.UriType.Localized,
				[typeof(char)] = LKeys.CharType.Localized,
				[typeof(bool)] = LKeys.BoolType.Localized,
				[typeof(sbyte)] = LKeys.SByteType.Localized,
				[typeof(byte)] = LKeys.ByteType.Localized,
				[typeof(short)] = LKeys.ShortType.Localized,
				[typeof(ushort)] = LKeys.UShortType.Localized,
				[typeof(int)] = LKeys.IntType.Localized,
				[typeof(uint)] = LKeys.UIntType.Localized,
				[typeof(long)] = LKeys.LongType.Localized,
				[typeof(ulong)] = LKeys.ULongType.Localized,
				[typeof(float)] = LKeys.FloatType.Localized,
				[typeof(double)] = LKeys.DoubleType.Localized,
				[typeof(decimal)] = LKeys.DecimalType.Localized,
				[typeof(DateTime)] = LKeys.DateTimeType.Localized,
				[typeof(DateTimeOffset)] = LKeys.DateTimeType.Localized,
				[typeof(TimeSpan)] = LKeys.TimeSpanType.Localized,
			};
		});
	}

	/// <inheritdoc />
	public override bool TryGetValue(Type key, [NotNullWhen(true)] out string value)
	{
		if (Items.TryGetValue(key, out value))
		{
			return true;
		}
		else if (key.IsGenericOf(typeof(Nullable<>)))
		{
			value = GenerateNullableName(key.GetGenericArguments()[0]);
		}
		else if (key.TryGetCollectionType(out var cType))
		{
			value = GenerateListName(cType);
		}
		// For sets, just return list name
		else if (key.TryGetHashSetType(out var sType))
		{
			value = GenerateListName(sType);
		}

		value ??= key.Name;
		Items.Add(key, value);
		return true;
	}

	/// <summary>
	/// Creates a <see cref="string"/> representing <paramref name="type"/> as an enumerable.
	/// </summary>
	/// <param name="type">The type to create a list name for.</param>
	/// <returns>A name for an enumerable of <paramref name="type"/>.</returns>
	protected virtual string GenerateListName(Type type)
	{
		var name = Items.TryGetValue(type, out var item) ? item : type.Name;
		return string.Format(LKeys.ListNameFormat.Localized, name);
	}

	/// <summary>
	/// Creates a <see cref="string"/> representing <paramref name="type"/> as a nullable.
	/// </summary>
	/// <param name="type">The type to create a nullable name for.</param>
	/// <returns>The name for a nullable of <see cref="Type"/>.</returns>
	protected virtual string GenerateNullableName(Type type)
	{
		var name = Items.TryGetValue(type, out var item) ? item : type.Name;
		return string.Format(LKeys.NullableNameFormat.Localized, name);
	}
}
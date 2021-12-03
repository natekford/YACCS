using System.Diagnostics.CodeAnalysis;

using YACCS.Localization;
using YACCS.TypeReaders;

namespace YACCS.Help;

/// <summary>
/// A registry for outputting the names of <see cref="Type"/>s to the end user.
/// </summary>
public class TypeNameRegistry : TypeRegistry<string>
{
	/// <summary>
	/// The localized instance backing <see cref="Items"/>.
	/// </summary>
	protected Localized<IDictionary<Type, string>> Localized = new(_ =>
	{
		return new Dictionary<Type, string>()
		{
			[typeof(string)] = Localization.Keys.StringType,
			[typeof(Uri)] = Localization.Keys.UriType,
			[typeof(char)] = Localization.Keys.CharType,
			[typeof(bool)] = Localization.Keys.BoolType,
			[typeof(sbyte)] = Localization.Keys.SByteType,
			[typeof(byte)] = Localization.Keys.ByteType,
			[typeof(short)] = Localization.Keys.ShortType,
			[typeof(ushort)] = Localization.Keys.UShortType,
			[typeof(int)] = Localization.Keys.IntType,
			[typeof(uint)] = Localization.Keys.UIntType,
			[typeof(long)] = Localization.Keys.LongType,
			[typeof(ulong)] = Localization.Keys.ULongType,
			[typeof(float)] = Localization.Keys.FloatType,
			[typeof(double)] = Localization.Keys.DoubleType,
			[typeof(decimal)] = Localization.Keys.DecimalType,
			[typeof(DateTime)] = Localization.Keys.DateTimeType,
			[typeof(DateTimeOffset)] = Localization.Keys.DateTimeType,
			[typeof(TimeSpan)] = Localization.Keys.TimeSpanType,
		};
	});

	/// <inheritdoc />
	protected override IDictionary<Type, string> Items => Localized.GetCurrent();

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
		return string.Format(Localization.Keys.ListNameFormat, name);
	}

	/// <summary>
	/// Creates a <see cref="string"/> representing <paramref name="type"/> as a nullable.
	/// </summary>
	/// <param name="type">The type to create a nullable name for.</param>
	/// <returns>The name for a nullable of <see cref="Type"/>.</returns>
	protected virtual string GenerateNullableName(Type type)
	{
		var name = Items.TryGetValue(type, out var item) ? item : type.Name;
		return string.Format(Localization.Keys.NullableNameFormat, name);
	}
}

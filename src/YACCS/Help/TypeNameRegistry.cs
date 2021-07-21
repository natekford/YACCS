using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using YACCS.Localization;
using YACCS.TypeReaders;

namespace YACCS.Help
{
	public class TypeNameRegistry : TypeRegistry<string>
	{
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

		protected override IDictionary<Type, string> Items => Localized.GetCurrent();

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

		protected virtual string GenerateListName(Type type)
		{
			var name = Items.TryGetValue(type, out var item) ? item : type.Name;
			return string.Format(Localization.Keys.ListNameFormat, name);
		}

		protected virtual string GenerateNullableName(Type type)
		{
			var name = Items.TryGetValue(type, out var item) ? item : type.Name;
			return string.Format(Localization.Keys.NullableNameFormat, name);
		}
	}
}
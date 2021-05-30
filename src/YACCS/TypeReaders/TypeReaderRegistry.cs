using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.NamedArguments;

namespace YACCS.TypeReaders
{
	public class TypeReaderRegistry : TypeRegistry<ITypeReader>
	{
		private static readonly MethodInfo _RegisterMethod =
			typeof(TypeReaderRegistry)
			.GetTypeInfo()
			.DeclaredMethods
			.Single(x => x.Name == nameof(RegisterWithNullable));

		public override ITypeReader this[Type key]
		{
			get => base[key];
			set
			{
				value.ThrowIfInvalidTypeReader(key);
				base[key] = value;
			}
		}

		public TypeReaderRegistry() : base(new Dictionary<Type, ITypeReader>())
		{
			Register(typeof(string), new StringTypeReader());
			Register(typeof(Uri), new UriTypeReader());
			RegisterWithNullable(new TryParseTypeReader<char>(char.TryParse));
			RegisterWithNullable(new TryParseTypeReader<bool>(bool.TryParse));
			RegisterWithNullable(new NumberTypeReader<sbyte>(sbyte.TryParse));
			RegisterWithNullable(new NumberTypeReader<byte>(byte.TryParse));
			RegisterWithNullable(new NumberTypeReader<short>(short.TryParse));
			RegisterWithNullable(new NumberTypeReader<ushort>(ushort.TryParse));
			RegisterWithNullable(new NumberTypeReader<int>(int.TryParse));
			RegisterWithNullable(new NumberTypeReader<uint>(uint.TryParse));
			RegisterWithNullable(new NumberTypeReader<long>(long.TryParse));
			RegisterWithNullable(new NumberTypeReader<ulong>(ulong.TryParse));
			RegisterWithNullable(new NumberTypeReader<float>(float.TryParse));
			RegisterWithNullable(new NumberTypeReader<double>(double.TryParse));
			RegisterWithNullable(new NumberTypeReader<decimal>(decimal.TryParse));
			RegisterWithNullable(new DateTimeTypeReader<DateTime>(DateTime.TryParse));
			RegisterWithNullable(new DateTimeTypeReader<DateTimeOffset>(DateTimeOffset.TryParse));
			RegisterWithNullable(new TimeSpanTypeReader<TimeSpan>(TimeSpan.TryParse));

			this.RegisterTypeReaders(typeof(TypeReaderRegistry).Assembly.GetTypeReaders());
		}

		public void Register(Type type, ITypeReader item)
		{
			if (type.IsValueType
				&& item.GetType().GetInterfaces().Any(x => x.IsGenericOf(typeof(ITypeReader<>))))
			{
				_RegisterMethod.MakeGenericMethod(type).Invoke(this, new object[] { item });
			}
			else
			{
				Items[type] = item;
			}
		}

		public void RegisterWithNullable<T>(ITypeReader<T> item) where T : struct
		{
			Items[typeof(T)] = item;
			Items[typeof(T?)] = new NullableTypeReader<T>(item);
		}

		public override bool TryGetValue(Type type, [NotNullWhen(true)] out ITypeReader reader)
		{
			if (Items.TryGetValue(type, out reader))
			{
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
			else if (type.GetCustomAttribute<GenerateNamedArgumentsAttribute>() is not null)
			{
				readerType = typeof(NamedArgumentTypeReader<>).MakeGenericType(type);
			}
			else if (type.GetArrayType() is Type eType && Items.ContainsKey(eType))
			{
				readerType = typeof(ArrayTypeReader<>).MakeGenericType(eType);
			}
			else
			{
				reader = null!;
				return false;
			}

			reader = readerType.CreateInstance<ITypeReader>();
			Register(type, reader);
			return true;
		}
	}
}
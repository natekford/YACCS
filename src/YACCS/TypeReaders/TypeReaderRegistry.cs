using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using YACCS.Commands;
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

		protected override IDictionary<Type, ITypeReader> Items { get; }
			= new Dictionary<Type, ITypeReader>();

		public TypeReaderRegistry(IEnumerable<Assembly>? assemblies = null)
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

			Items.RegisterTypeReaders(typeof(TypeReaderRegistry).Assembly.GetTypeReaders());
			if (assemblies is not null)
			{
				foreach (var assembly in assemblies)
				{
					Items.RegisterTypeReaders(assembly.GetTypeReaders());
				}
			}
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
			Items[typeof(T?)] = new NullableTypeReader<T>();
		}

		public override bool TryGetValue(Type type, [NotNullWhen(true)] out ITypeReader reader)
		{
			if (Items.TryGetValue(type, out reader))
			{
				return true;
			}
			if (!TryGetReaderType(type, out var readerType))
			{
				reader = null!;
				return false;
			}

			reader = readerType.CreateInstance<ITypeReader>();
			Register(type, reader);
			return true;
		}

		protected virtual bool TryGetReaderType(Type type, [NotNullWhen(true)] out Type? readerType)
		{
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
				readerType = typeof(NamedArgumentsTypeReader<>).MakeGenericType(type);
			}
			else if (type.GetListType() is Type eType && Items.ContainsKey(eType))
			{
				var typeDef = type.IsArray ? typeof(ArrayTypeReader<>) : typeof(ListTypeReader<>);
				readerType = typeDef.MakeGenericType(eType);
			}
			else
			{
				readerType = null;
			}
			return readerType is not null;
		}
	}
}
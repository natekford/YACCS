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
		private static readonly MethodInfo _RegisterNullable =
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
			if (type.IsValueType)
			{
				_RegisterNullable.MakeGenericMethod(type).Invoke(this, new object[] { item });
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
			if (TryCreateReader(type, out reader))
			{
				Register(type, reader);
				return true;
			}
			return false;
		}

		protected virtual bool TryCreateReader(Type type, [NotNullWhen(true)] out ITypeReader reader)
		{
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
				readerType = typeof(NamedArgumentsTypeReader<>).MakeGenericType(type);
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
				reader = null!;
				return false;
			}

			reader = readerType.CreateInstance<ITypeReader>();
			return true;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.NamedArguments;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public class TypeReaderRegistry : ITypeRegistry<ITypeReader>
	{
		private static readonly MethodInfo _RegisterMethod =
			typeof(TypeReaderRegistry)
			.GetTypeInfo()
			.DeclaredMethods
			.Single(x => x.Name == nameof(RegisterWithNullable));

		private readonly Dictionary<Type, ITypeReader> _Readers
			= new Dictionary<Type, ITypeReader>();

		public TypeReaderRegistry()
		{
			this.Register(new StringTypeReader());
			this.Register(new UriTypeReader());
			this.Register(new ContextTypeReader<IContext>());
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

			this.Register(typeof(TypeReaderRegistry).Assembly.GetTypeReaders());
		}

		public void Register(Type type, ITypeReader item)
		{
			item.ThrowIfInvalidTypeReader(type);

			if (type.IsValueType
				&& item.GetType().GetInterfaces().Any(x => x.IsGenericOf(typeof(ITypeReader<>))))
			{
				_RegisterMethod.MakeGenericMethod(type).Invoke(this, new object[] { item });
			}

			_Readers[type] = item;
		}

		public void RegisterWithNullable<T>(ITypeReader<T> item) where T : struct
		{
			_Readers[typeof(T)] = item;
			_Readers[typeof(T?)] = new NullableTypeReader<T>(item);
		}

		public bool TryGet(Type type, [NotNullWhen(true)] out ITypeReader? item)
		{
			if (_Readers.TryGetValue(type, out item))
			{
				return true;
			}

			Type readerType;
			if (type.IsEnum)
			{
				readerType = typeof(EnumTypeReader<>).MakeGenericType(type);
			}
			else if (type.GetCustomAttribute<GenerateNamedArgumentsAttribute>() != null)
			{
				readerType = typeof(NamedArgumentTypeReader<>).MakeGenericType(type);
			}
			else
			{
				item = null;
				return false;
			}

			item = (ITypeReader)Activator.CreateInstance(readerType);
			Register(type, item);
			return true;
		}
	}
}
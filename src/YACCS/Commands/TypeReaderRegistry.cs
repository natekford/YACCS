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
	public class TypeReaderRegistry : ITypeReaderRegistry
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

		public void Register(ITypeReader reader, Type type)
		{
			reader.ThrowIfInvalidTypeReader(type);

			if (type.IsValueType
				&& reader.GetType().GetInterfaces().Any(x => x.IsGenericOf(typeof(ITypeReader<>))))
			{
				_RegisterMethod.MakeGenericMethod(type).Invoke(this, new object[] { reader });
			}

			_Readers[type] = reader;
		}

		public void RegisterWithNullable<T>(ITypeReader<T> reader) where T : struct
		{
			_Readers[typeof(T)] = reader;
			_Readers[typeof(T?)] = new NullableTypeReader<T>(reader);
		}

		public bool TryGetReader(Type type, [NotNullWhen(true)] out ITypeReader? result)
		{
			if (_Readers.TryGetValue(type, out result))
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
				result = null;
				return false;
			}

			result = (ITypeReader)Activator.CreateInstance(readerType);
			Register(result, type);
			return true;
		}
	}
}
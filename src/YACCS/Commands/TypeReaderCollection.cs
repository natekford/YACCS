using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public class TypeReaderCollection : ITypeReaderCollection
	{
		private static readonly MethodInfo _RegisterMethod =
			typeof(TypeReaderCollection)
			.GetTypeInfo()
			.DeclaredMethods
			.Single(x => x.Name == nameof(RegisterWithNullable));

		private readonly Dictionary<Type, ITypeReader> _Readers
			= new Dictionary<Type, ITypeReader>();

		public TypeReaderCollection()
		{
			Register(new StringTypeReader());
			Register(new UriTypeReader());
			Register(new ContextTypeReader<IContext>());
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
		}

		public ITypeReader GetReader(Type type)
		{
			if (!_Readers.TryGetValue(type, out var reader))
			{
				throw new ArgumentException("There is no converter specified for given type.", type.Name);
			}
			return reader;
		}

		public ITypeReader<T> GetReader<T>()
		{
			var reader = GetReader(typeof(T));
			if (!(reader is ITypeReader<T> castedReader))
			{
				throw new ArgumentException("Invalid converter registered for this type.", nameof(T));
			}
			return castedReader;
		}

		public void Register<T>(ITypeReader<T> reader)
			=> Register(reader, typeof(T));

		public void Register(ITypeReader reader, Type type)
		{
			_Readers[type] = reader;

			if (type.IsValueType)
			{
				_RegisterMethod.MakeGenericMethod(type).Invoke(this, new object[] { reader });
			}
		}

		public void RegisterWithNullable<T>(ITypeReader<T> reader) where T : struct
		{
			_Readers[typeof(T)] = reader;
			_Readers[typeof(T?)] = new NullableTypeReader<T>(reader);
		}

		public bool TryGetReader(Type type, out ITypeReader result)
			=> _Readers.TryGetValue(type, out result);
	}
}
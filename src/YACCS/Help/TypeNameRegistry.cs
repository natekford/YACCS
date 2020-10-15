using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;

namespace YACCS.Help
{
	public class TypeNameRegistry : ITypeRegistry<string>
	{
		private readonly Dictionary<Type, string> _Names
			= new Dictionary<Type, string>();

		public TypeNameRegistry()
		{
			Register(typeof(string), "text");
			Register(typeof(Uri), "url");
			//Register(typeof(IContext), "");
			Register(typeof(char), "char");
			Register(typeof(bool), "true or false");
			Register(typeof(sbyte), $"integer ({sbyte.MinValue} to {sbyte.MaxValue})");
			Register(typeof(byte), $"integer ({byte.MinValue} to {byte.MaxValue})");
			Register(typeof(short), $"integer ({short.MinValue} to {short.MaxValue})");
			Register(typeof(ushort), $"integer ({ushort.MinValue} to {ushort.MaxValue})");
			Register(typeof(int), $"integer ({int.MinValue} to {int.MaxValue})");
			Register(typeof(uint), $"integer ({uint.MinValue} to {uint.MaxValue})");
			Register(typeof(long), $"integer ({long.MinValue} to {long.MaxValue})");
			Register(typeof(ulong), $"integer ({ulong.MinValue} to {ulong.MaxValue})");
			Register(typeof(float), $"number ({float.MinValue} to {float.MaxValue})");
			Register(typeof(double), $"number ({double.MinValue} to {double.MaxValue})");
			Register(typeof(decimal), $"number ({decimal.MinValue} to {decimal.MaxValue})");
			Register(typeof(DateTime), "date");
			Register(typeof(DateTimeOffset), "date");
			Register(typeof(TimeSpan), "time");
		}

		public void Register(Type type, string item)
			=> _Names[type] = item;

		public bool TryGet(Type type, [MaybeNull, NotNullWhen(true)] out string item)
		{
			if (_Names.TryGetValue(type, out item))
			{
				return true;
			}

			var eType = type.GetEnumerableType();
			if (eType is not null)
			{
				var eName = _Names.TryGetValue(eType, out var eItem) ? eItem : type.Name;
				item = "List of " + eName;
			}

			Register(type, item);
			return true;
		}
	}
}
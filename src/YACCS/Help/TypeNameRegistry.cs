using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Help
{
	public class TypeNameRegistry : ITypeRegistry<string>
	{
		private readonly Dictionary<Type, string> _Names = new Dictionary<Type, string>
		{
			{ typeof(string), "text" },
			{ typeof(Uri), "url" },
			//{ typeof(IContext), "context" },
			{ typeof(char), "char" },
			{ typeof(bool), "true or false" },
			{ typeof(sbyte), $"integer ({sbyte.MinValue} to {sbyte.MaxValue})" },
			{ typeof(byte), $"integer ({byte.MinValue} to {byte.MaxValue})" },
			{ typeof(short), $"integer ({short.MinValue} to {short.MaxValue})" },
			{ typeof(ushort), $"integer ({ushort.MinValue} to {ushort.MaxValue})" },
			{ typeof(int), $"integer ({int.MinValue} to {int.MaxValue})" },
			{ typeof(uint), $"integer ({uint.MinValue} to {uint.MaxValue})" },
			{ typeof(long), $"integer ({long.MinValue} to {long.MaxValue})" },
			{ typeof(ulong), $"integer ({ulong.MinValue} to {ulong.MaxValue})" },
			{ typeof(float), $"number ({float.MinValue} to {float.MaxValue})" },
			{ typeof(double), $"number ({double.MinValue} to {double.MaxValue})" },
			{ typeof(decimal), $"number ({decimal.MinValue} to {decimal.MaxValue})" },
			{ typeof(DateTime), "date" },
			{ typeof(DateTimeOffset), "date" },
			{ typeof(TimeSpan), "time" },
		};

		public void Register(Type type, string item)
			=> _Names[type] = item;

		public bool TryGet(Type type, [MaybeNull, NotNullWhen(true)] out string item)
		{
			if (_Names.TryGetValue(type, out item))
			{
				return true;
			}
			else if (type.IsGenericOf(typeof(Nullable<>)))
			{
				item = GenerateNullableName(type.GetGenericArguments()[0]);
			}
			else if (type.GetEnumerableType() is Type eType)
			{
				item = GenerateListName(eType);
			}

			item ??= type.Name;
			Register(type, item);
			return true;
		}

		protected virtual string GenerateListName(Type type)
		{
			var name = _Names.TryGetValue(type, out var item) ? item : type.Name;
			return name + " list";
		}

		protected virtual string GenerateNullableName(Type type)
		{
			var name = _Names.TryGetValue(type, out var item) ? item : type.Name;
			return name + " or null";
		}
	}
}
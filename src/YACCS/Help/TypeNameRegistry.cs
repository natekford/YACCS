using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Help
{
	public class TypeNameRegistry : TypeRegistry<string>
	{
		public TypeNameRegistry() : base(new Dictionary<Type, string>()
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
		})
		{
		}

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
			else if (key.GetArrayType() is Type eType)
			{
				value = GenerateListName(eType);
			}

			value ??= key.Name;
			Add(key, value);
			return true;
		}

		protected virtual string GenerateListName(Type type)
		{
			var name = Items.TryGetValue(type, out var item) ? item : type.Name;
			return name + " list";
		}

		protected virtual string GenerateNullableName(Type type)
		{
			var name = Items.TryGetValue(type, out var item) ? item : type.Name;
			return name + " or null";
		}
	}
}
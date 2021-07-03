using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class EntityBase : IEntityBase
	{
		public IList<object> Attributes { get; set; } = new List<object>();
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		private string DebuggerDisplay => $"Attribute Count = {Attributes.Count}";

		protected EntityBase(ICustomAttributeProvider? provider)
		{
			AddAttributes(provider);
		}

		protected void AddAttributes(ICustomAttributeProvider? provider)
		{
			if (provider is not null)
			{
				foreach (var attribute in provider.GetCustomAttributes(true))
				{
					Attributes.Add(attribute);
				}
			}
		}
	}

	internal static class EntityBaseUtils
	{
		internal static TValue ThrowIfDuplicate<TAttribute, TValue>(
			this TAttribute attribute,
			Func<TAttribute, TValue> converter,
			ref int count)
		{
			if (count > 0)
			{
				throw new InvalidOperationException(
					$"Duplicate {typeof(TAttribute).Name} attribute.");
			}

			++count;
			return converter.Invoke(attribute);
		}
	}
}
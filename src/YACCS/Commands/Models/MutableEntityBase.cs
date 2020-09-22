using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class MutableEntityBase : IMutableEntityBase
	{
		public IList<object> Attributes { get; set; } = new List<object>();
		public string Id { get; set; }
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		private string DebuggerDisplay => $"Id = {Id}, Attribute Count = {Attributes.Count}";

		protected MutableEntityBase(ICustomAttributeProvider provider)
		{
			foreach (var attribute in provider.GetCustomAttributes(true))
			{
				Attributes.Add(attribute);
			}

			Id = Get<IIdAttribute>().SingleOrDefault()?.Id ?? Guid.NewGuid().ToString();
		}

		public IEnumerable<T> Get<T>()
			=> Attributes.OfType<T>();
	}
}
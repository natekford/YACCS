using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public abstract class EntityBase : IEntityBase
	{
		public IList<object> Attributes { get; set; }
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		protected ImmutableArray<object> BaseAttributes { get; }
		private string DebuggerDisplay => $"Attribute Count = {Attributes.Count}";

		protected EntityBase(ICustomAttributeProvider? provider)
		{
			var attributes = Array.Empty<object>();
			if (provider is not null)
			{
				attributes = provider.GetCustomAttributes(true);
			}

			Attributes = new List<object>(attributes);
			BaseAttributes = Unsafe.As<object[], ImmutableArray<object>>(ref attributes);
		}
	}
}
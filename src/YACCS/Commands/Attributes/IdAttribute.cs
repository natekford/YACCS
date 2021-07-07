using System;
using System.Threading.Tasks;

using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class IdAttribute : Attribute, IIdAttribute, IRuntimeFormattableAttribute
	{
		public virtual string Id { get; }

		public IdAttribute(string id)
		{
			Id = id;
		}

		public virtual ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
			=> new(formatProvider.Format($"{Keys.ID:k} {Id:v}"));
	}
}
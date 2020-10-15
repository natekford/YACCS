using System;

namespace YACCS.Help.Models
{
	public class HelpType : HelpItem<Type>
	{
		public HelpType(Type item) : base(item, item.GetCustomAttributes(true))
		{
		}
	}
}
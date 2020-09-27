using System;

using YACCS.Commands.Models;

namespace YACCS.Tests
{
	public static class FakeDelegateCommand
	{
		public static DelegateCommand New
			=> new DelegateCommand((Action)(() => { }), Array.Empty<IName>());

		public static DelegateCommand WithAttribute(this DelegateCommand command, object attribute)
		{
			command.Attributes.Add(attribute);
			return command;
		}
	}
}
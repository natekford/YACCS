using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help;

namespace YACCS.Tests.Help
{
	[TestClass]
	public class Model_Tests
	{
		[TestMethod]
		public void HasContextType_Test()
		{
			var (formatter, command) = Create();
			var helpCommand = formatter.GetHelpCommand(command.MakeImmutable());
			Assert.AreEqual(typeof(IContext), helpCommand.ContextType.Item);
		}

		private (HelpFormatter, DelegateCommand) Create()
		{
			var formatter = new HelpFormatter(new TypeNameRegistry());
			var command = FakeDelegateCommand.New();
			return (formatter, command);
		}
	}
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Help.Models;
using YACCS.TypeReaders;

namespace YACCS.Tests.Help
{
	[TestClass]
	public class Model_Tests
	{
		[TestMethod]
		public void HasContextType_Test()
		{
			var command = FakeDelegateCommand.New();
			var helpCommand = new HelpCommand(command.ToImmutable());
			Assert.AreEqual(typeof(IContext), helpCommand.ContextType.Item);
		}

		[TestMethod]
		public void SummaryAndName_Test()
		{
			var command = FakeDelegateCommand.New();
			var summary = new SummaryAttribute("idk lol summary");
			command.Attributes.Add(summary);
			var name = new NameAttribute("idk lol name");
			command.Attributes.Add(name);
			var helpCommand = new HelpCommand(command.ToImmutable());

			Assert.AreSame(summary, helpCommand.Summary);
			Assert.AreSame(name, helpCommand.Name);
		}

		[TestMethod]
		public void TypeReader_Test()
		{
			var parameter = new Parameter(typeof(string), "nothing", null);

			var helpParameter1 = new HelpParameter(parameter.ToImmutable());
			Assert.IsNull(helpParameter1.TypeReader);

			parameter.TypeReader = new StringTypeReader();
			var helpParameter2 = new HelpParameter(parameter.ToImmutable());
			Assert.AreSame(parameter.TypeReader, helpParameter2.TypeReader!.Item);
		}
	}
}
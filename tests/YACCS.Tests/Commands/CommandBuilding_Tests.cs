#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0022 // Use expression body for methods

using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandBuilding_Tests
	{
		[TestMethod]
		public async Task CommandDelegateBuilding_Test()
		{
			var @delegate = (Func<IContext, Task<bool>>)((IContext arg) => Task.FromResult(true));
			var names = new[] { new Name(new[] { "Joe" }) };
			var command = new DelegateCommand(@delegate, names);
			var immutable = command.ToCommand();
			Assert.AreEqual(names.Length, command.Names.Count);
			Assert.AreEqual(names[0], command.Names[0]);
			Assert.AreEqual(1, command.Parameters.Count);
			Assert.AreEqual(1, command.Attributes.Count);
			Assert.IsInstanceOfType(command.Attributes[0], typeof(DelegateCommandAttribute));

			var args = new object[] { new FakeContext() };
			var result = await immutable.GetResultAsync(null, args).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Result, typeof(ValueResult));
			Assert.IsTrue(result.TryGetValue(out bool value));
			Assert.AreEqual(true, value);
		}

		[TestMethod]
		public async Task CommandMethodInfoBuilding_Test()
		{
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.Commands.Linq;
using System.Linq;

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
			var result = await immutable.ExecuteAsync(null!, args).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Result, typeof(ValueResult));
			Assert.IsTrue(result.TryGetValue(out bool value));
			Assert.AreEqual(true, value);
		}

		[TestMethod]
		public async Task CommandMethodInfoBuilding_Test()
		{
			var commands = new List<IImmutableCommand>();
			await foreach (var command in typeof(GroupBase).GetCommandsAsync())
			{
				commands.Add(command);
			}
			Assert.AreEqual(2, commands.Count);

			var command1 = commands.ById(GroupBase.ID_1).SingleOrDefault();
			Assert.IsNotNull(command1);

			var command2 = commands.ById(GroupBase.ID_2).SingleOrDefault();
			Assert.IsNotNull(command2);
		}

		[TestMethod]
		public async Task CommandMethodInfoBuildingWithInheritanceInvolved_Test()
		{
			var commands = new List<IImmutableCommand>();
			await foreach (var command in typeof(GroupChild).GetCommandsAsync())
			{
				commands.Add(command);
			}
			Assert.AreEqual(1, commands.Count);

			var command1 = commands.ById(GroupBase.ID_1).SingleOrDefault();
			Assert.IsNotNull(command1);

			var command2 = commands.ById(GroupBase.ID_2).SingleOrDefault();
			Assert.IsNull(command2);
		}
	}

	public class GroupBase : CommandGroup<FakeContext>
	{
		public const string ID_1 = "id_1";
		public const string ID_2 = "id_2";

		[Command("joeba", AllowInheritance = true)]
		[Id(ID_1)]
		public Task<IResult> CommandAsync() => SuccessResult.InstanceTask;

		[Command("joeba2", AllowInheritance = false)]
		[Id(ID_2)]
		public Task<IResult> CommandAsync2() => SuccessResult.InstanceTask;
	}

	public class GroupChild : GroupBase
	{
	}
}
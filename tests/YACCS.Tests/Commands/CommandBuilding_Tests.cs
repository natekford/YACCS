#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0022 // Use expression body for methods

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
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

		[TestMethod]
		public async Task CommandQuerying_Test()
		{
			var commands = new List<ICommand>();
			await foreach (var command in typeof(Misc).GetCommandsAsync())
			{
				commands.Add(command);
			}

			Assert.AreEqual(4, commands.Count);

			var byId = commands.ById(Misc._CommandTwoId).ToArray();
			Assert.AreEqual(1, byId.Length,
				"Received wrong count of commands when searching by id.");

			var t = new Misc.Help();
			var @delegate = (Func<IReadOnlyList<string>, Task<IResult>>)t.CommandFour;

			var byDelegate1 = commands.ByDelegate(@delegate, false).ToArray();
			Assert.AreEqual(0, byDelegate1.Length,
				"Received wrong count of commands when searching by delegate.");

			var byDelegate2 = commands.ByDelegate(@delegate, true).ToArray();
			Assert.AreEqual(1, byDelegate2.Length,
				"Received wrong count of commands when searching by delegate (including method).");

			var byMethod = commands.ByMethod(@delegate.Method).ToArray();
			Assert.AreEqual(1, byMethod.Length,
				"Received wrong count of commands when searching by method.");

			var byLastPartOfName = commands.ByLastPartOfName(Misc._7).ToArray();
			Assert.AreEqual(1, byLastPartOfName.Length,
				"Received wrong count of commands when searching by last part of name.");

			var byName = commands.ByName(new[] { Misc._1, Misc._4 }).ToArray();
			Assert.AreEqual(3, byName.Length,
				"Received wrong count of commands when searching by name.");
		}
	}

	[Command(_1, _2, _3)]
	public sealed class Misc : CommandGroup<IContext>
	{
		public const string _1 = "1";
		public const string _2 = "2";
		public const string _3 = "3";
		public const string _4 = "4";
		public const string _5 = "5";
		public const string _6 = "6";
		public const string _7 = "7";
		public const string _8 = "8";
		public const string _9 = "9";
		public const string _CommandOneId = "id_1";
		public const string _CommandTwoId = "id_2";

		[Command(_4, _5, _6)]
		public sealed class Help : CommandGroup<IContext>
		{
			[Command(_7, _8, _9)]
			[Id(_CommandOneId)]
			public Task<IResult> CommandOne()
			{
				return SuccessResult.InstanceTask;
			}

			[Command]
			[Id(_CommandTwoId)]
			public Task<IResult> CommandTwo(string arg)
			{
				return SuccessResult.InstanceTask;
			}

			[Command]
			public Task<IResult> CommandThree(int position, string arg)
			{
				return SuccessResult.InstanceTask;
			}

			[Command]
			public Task<IResult> CommandFour(IReadOnlyList<string> list)
			{
				return SuccessResult.InstanceTask;
			}
		}
	}
}
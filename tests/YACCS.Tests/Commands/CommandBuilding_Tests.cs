#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0022 // Use expression body for methods

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.Commands.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandBuilding_Tests
	{
		[TestMethod]
		public async Task TestMethod1()
		{
			var commands = new List<ICommand>();
			await foreach (var command in CommandServiceUtils.GetCommandsAsync(typeof(Misc)))
			{
				commands.Add(command);
			}

			Assert.AreEqual(4, commands.Count);
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
		public const string _Id = "id";

		[Command(_4, _5, _6)]
		public sealed class Help : CommandGroup<IContext>
		{
			[Command(_7, _8, _9)]
			public Task<IResult> CommandOne()
			{
				return SuccessResult.InstanceTask;
			}

			[Command]
			[Id(_Id)]
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

			public override Task OnCommandBuildingAsync(IList<IMutableCommand> commands)
			{
				var byId = commands.ById(_Id).ToArray();
				Assert.AreEqual(1, byId.Length,
					"Received wrong count of commands when searching by id.");

				var @delegate = (Func<IReadOnlyList<string>, Task<IResult>>)CommandFour;

				var byDelegate1 = commands.ByDelegate(@delegate, false).ToArray();
				Assert.AreEqual(0, byDelegate1.Length,
					"Received wrong count of commands when searching by delegate.");

				var byDelegate2 = commands.ByDelegate(@delegate, true).ToArray();
				Assert.AreEqual(1, byDelegate2.Length,
					"Received wrong count of commands when searching by delegate (including method).");

				var byMethod = commands.ByMethod(@delegate.Method).ToArray();
				Assert.AreEqual(1, byMethod.Length,
					"Received wrong count of commands when searching by method.");

				var byLastPartOfName = commands.ByLastPartOfName(_7).ToArray();
				Assert.AreEqual(1, byLastPartOfName.Length,
					"Received wrong count of commands when searching by last part of name.");

				var byName = commands.ByName(new[] { _1, _4 }).ToArray();
				Assert.AreEqual(3, byLastPartOfName.Length,
					"Received wrong count of commands when searching by name.");

				return Task.CompletedTask;
			}
		}
	}
}
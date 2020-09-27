using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Tests.Commands.Linq
{
	[TestClass]
	public class Querying_Tests
	{
		[TestMethod]
		public async Task ByDelegate_Test()
		{
			var commands = await CreateCommandsAsync().ConfigureAwait(false);

			var t = new Querying_TestsGroup.Help();
			var @delegate = (Func<IReadOnlyList<string>, Task<IResult>>)t.CommandFour;

			var result = commands.ByDelegate(@delegate, includeMethod: false).ToArray();
			Assert.AreEqual(0, result.Length);

			var command = new DelegateCommand(@delegate, Array.Empty<IName>());
			commands.Add(command.ToCommand());

			var result2 = commands.ByDelegate(@delegate, includeMethod: false).ToArray();
			Assert.AreEqual(1, result2.Length);
		}

		[TestMethod]
		public async Task ByDelegateIncludingMethod_Test()
		{
			var commands = await CreateCommandsAsync().ConfigureAwait(false);

			var t = new Querying_TestsGroup.Help();
			var @delegate = (Func<IReadOnlyList<string>, Task<IResult>>)t.CommandFour;

			var result = commands.ByDelegate(@delegate, includeMethod: true).ToArray();
			Assert.AreEqual(1, result.Length);
		}

		[TestMethod]
		public async Task ById_Test()
		{
			var commands = await CreateCommandsAsync().ConfigureAwait(false);

			var result = commands.ById(Querying_TestsGroup._CommandTwoId).ToArray();
			Assert.AreEqual(1, result.Length);
		}

		[TestMethod]
		public async Task ByLastPartOfName_Test()
		{
			var commands = await CreateCommandsAsync().ConfigureAwait(false);

			var result = commands.ByLastPartOfName(Querying_TestsGroup._7).ToArray();
			Assert.AreEqual(1, result.Length);
		}

		[TestMethod]
		public async Task ByMethod_Test()
		{
			var commands = await CreateCommandsAsync().ConfigureAwait(false);

			var method = typeof(Querying_TestsGroup.Help)
				.GetMethod(nameof(Querying_TestsGroup.Help.CommandFour));
			var result = commands.ByMethod(method!).ToArray();
			Assert.AreEqual(1, result.Length);
		}

		[TestMethod]
		public async Task ByName_Test()
		{
			var commands = await CreateCommandsAsync().ConfigureAwait(false);

			{
				var result = commands.ByName(new[]
				{
					Querying_TestsGroup._1,
					Querying_TestsGroup._4
				}).ToArray();
				Assert.AreEqual(3, result.Length);
			}

			{
				var result = commands.ByName(new[]
				{
					Querying_TestsGroup._1.ToUpper(),
					Querying_TestsGroup._4
				}).ToArray();
				Assert.AreEqual(0, result.Length);
			}

			{
				var result = commands.ByName(new[]
				{
					Querying_TestsGroup._1.ToUpper(),
					Querying_TestsGroup._4
				}, StringComparison.OrdinalIgnoreCase).ToArray();
				Assert.AreEqual(3, result.Length);
			}
		}

		private async Task<List<IImmutableCommand>> CreateCommandsAsync()
		{
			var commands = new List<IImmutableCommand>();
			await foreach (var command in typeof(Querying_TestsGroup).GetCommandsAsync())
			{
				commands.Add(command);
			}

			Assert.AreEqual(4, commands.Count);
			return commands;
		}

		[Command(_1, _2, _3)]
		private sealed class Querying_TestsGroup : CommandGroup<IContext>
		{
			public const string _1 = "one";
			public const string _2 = "two";
			public const string _3 = "three";
			public const string _4 = "four";
			public const string _5 = "five";
			public const string _6 = "six";
			public const string _7 = "seven";
			public const string _8 = "eight";
			public const string _9 = "nine";
			public const string _CommandOneId = "id_1";
			public const string _CommandTwoId = "id_2";
			public const string _PositionId = "position_id";

			[Command(_4, _5, _6)]
			public sealed class Help : CommandGroup<IContext>
			{
				[Command]
				public Task<IResult> CommandFour(IReadOnlyList<string> list)
					=> SuccessResult.InstanceTask;

				[Command(_7, _8, _9)]
				[Id(_CommandOneId)]
				public Task<IResult> CommandOne()
					=> SuccessResult.InstanceTask;

				[Command]
				public Task<IResult> CommandThree([Id(_PositionId)] int position, string arg)
					=> SuccessResult.InstanceTask;

				[Command]
				[Id(_CommandTwoId)]
				public Task<IResult> CommandTwo(string arg) => SuccessResult.InstanceTask;

				public override Task OnCommandBuildingAsync(IList<ICommand> commands)
				{
					var parameters = commands.SelectMany(x => x.Parameters);
					var position = parameters.GetParameterById<int>(_PositionId);
					Assert.IsNotNull(position);

					return Task.CompletedTask;
				}
			}
		}
	}
}
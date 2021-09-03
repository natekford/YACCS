using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Tests.Commands.Linq
{
	[TestClass]
	public class Entity_Tests
	{
		[TestMethod]
		public async Task ByDelegate_Test()
		{
			var commands = await CreateCommandsAsync().ConfigureAwait(false);

			var t = new Querying_TestsGroup.Help();
			var @delegate = (Func<IReadOnlyList<string>, IResult>)t.CommandFour;

			{
				var result = commands.ByDelegate(@delegate, includeMethod: true).ToArray();
				Assert.AreEqual(1, result.Length);
			}

			{
				var result = commands.ByDelegate(@delegate, includeMethod: false).ToArray();
				Assert.AreEqual(0, result.Length);
			}

			{
				var command = new DelegateCommand(@delegate, Array.Empty<ImmutableName>());
				commands.Add(command.ToImmutable());

				var result = commands.ByDelegate(@delegate, includeMethod: false).ToArray();
				Assert.AreEqual(1, result.Length);
			}
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

		private async ValueTask<List<IImmutableCommand>> CreateCommandsAsync()
		{
			var wasIReached = new WasIReached();
			var services = new ServiceCollection()
				.AddSingleton(wasIReached)
				.BuildServiceProvider();

			var commands = new List<IImmutableCommand>();
			await foreach (var (_, command) in typeof(Querying_TestsGroup).GetAllCommandsAsync(services))
			{
				commands.Add(command);
			}

			Assert.AreEqual(4, commands.Count);
			Assert.IsTrue(wasIReached.WasReached);
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
			[HelpOnCommandBuilding]
			public sealed class Help : CommandGroup<IContext>
			{
				[Command]
				public IResult CommandFour(IReadOnlyList<string> list)
					=> SuccessResult.Instance;

				[Command(_7, _8, _9)]
				[Id(_CommandOneId)]
				public IResult CommandOne()
					=> SuccessResult.Instance;

				[Command]
				public IResult CommandThree([Id(_PositionId)] int position, string arg)
					=> SuccessResult.Instance;

				[Command]
				[Id(_CommandTwoId)]
				public IResult CommandTwo(string arg)
					=> SuccessResult.Instance;

				[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
				private class HelpOnCommandBuildingAttribute : OnCommandBuildingAttribute
				{
					public override Task ModifyCommands(IServiceProvider services, List<ReflectionCommand> commands)
					{
						var parameters = commands.SelectMany(x => x.Parameters);
						var position = parameters.ById(_PositionId).Single().AsType<int>();
						Assert.IsNotNull(position);

						services.GetRequiredService<WasIReached>().WasReached = true;
						return Task.CompletedTask;
					}
				}
			}
		}

		private sealed class WasIReached
		{
			public bool WasReached { get; set; }
		}
	}
}
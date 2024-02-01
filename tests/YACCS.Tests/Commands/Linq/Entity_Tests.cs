using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Tests.Commands.Linq;

[TestClass]
public class Entity_Tests
{
	[TestMethod]
	public async Task ById_Test()
	{
		var commands = await CreateCommandsAsync().ConfigureAwait(false);

		var result = commands.ById(Querying_TestsGroup._CommandTwoId).ToArray();
		Assert.AreEqual(1, result.Length);
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
		public sealed class Help : CommandGroup<IContext>
		{
			[Command]
			public Success CommandFour(IReadOnlyList<string> list) => Success.Instance;

			[Command(_7, _8, _9)]
			[Id(_CommandOneId)]
			public Success CommandOne() => Success.Instance;

			[Command]
			public Success CommandThree([Id(_PositionId)] int position, string arg) => Success.Instance;

			[Command]
			[Id(_CommandTwoId)]
			public Success CommandTwo(string arg) => Success.Instance;

			public override Task ModifyCommandsAsync(IServiceProvider services, List<ReflectionCommand> commands)
			{
				var parameters = commands.SelectMany(x => x.Parameters);
				var position = parameters.ById(_PositionId).Single().AsType<int>();
				Assert.IsNotNull(position);

				services.GetRequiredService<WasIReached>().WasReached = true;
				return base.ModifyCommandsAsync(services, commands);
			}
		}
	}

	private sealed class WasIReached
	{
		public bool WasReached { get; set; }
	}
}
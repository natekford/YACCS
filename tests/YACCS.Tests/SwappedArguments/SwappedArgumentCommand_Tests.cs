using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.SwappedArguments;

namespace YACCS.Tests.SwappedArguments;

[TestClass]
public class SwappedArgumentCommand_Tests
{
	private const int AMOUNT = 73;
	private const bool CHANNEL = false;
	private const char USER = 'c';
	private static readonly DateTime TIME = new(2000, 1, 1, 12, 30, 30);

	[TestMethod]
	public async Task AllPermutationsSwapArguments_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		Assert.AreEqual(3 * 2 * 1, commandService.Commands.Count);

		var args = new[]
		{
			AMOUNT.ToString(),
			USER.ToString(),
			CHANNEL.ToString(),
			$"\"{TIME}\"",
		};

		var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		commandService.CommandExecuted += (e) =>
		{
			tcs.SetResult();
			return Task.CompletedTask;
		};

		foreach (var swapper in Swapper.CreateSwappers(new[] { 1, 2, 3 }))
		{
			var copy = args.ToArray();
			swapper.Swap(copy);

			var input = string.Join(' ', copy.Prepend(nameof(CommandsGroup.RemoveMessages)));
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);

			await tcs.Task.ConfigureAwait(false);
			Assert.AreEqual(AMOUNT, setMe.Amount);
			Assert.AreEqual(USER, setMe.User);
			Assert.AreEqual(CHANNEL, setMe.Channel);
			Assert.AreEqual(TIME, setMe.Time);

			setMe.Reset();
			tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		}
	}

	[TestMethod]
	public async Task ThrowsWhenTryingToSwapRemainder()
	{
		var commands = typeof(CommandsGroupThrow).GetAllCommandsAsync(FakeServiceProvider.Instance);

		var i = 0;
		await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
		{
			await foreach (var command in commands)
			{
				++i;
			}
		}).ConfigureAwait(false);
		// The first command should be built fine, the first swap should throw due to
		// trying to swap a remainder
		Assert.AreEqual(1, i);
	}

	private async ValueTask<(FakeCommandService, SetMe, FakeContext)> CreateAsync()
	{
		var setMe = new SetMe();
		var context = new FakeContext()
		{
			Services = Utils.CreateServiceCollection().AddSingleton(setMe).BuildServiceProvider(),
		};

		var commandService = context.Get<FakeCommandService>();
		var commands = typeof(CommandsGroup).GetAllCommandsAsync(context.Services);
		await commandService.AddRangeAsync(commands).ConfigureAwait(false);

		return (commandService, setMe, context);
	}

	private class CommandsGroup : CommandGroup<IContext>
	{
		[InjectService]
		public SetMe SetMe { get; set; } = null!;

		[Command(nameof(RemoveMessages))]
		[GenerateSwappedArguments]
		public void RemoveMessages(
			int amount,
			[Swappable]
			char? user = null,
			[Swappable]
			bool? channel = null,
			[Swappable]
			DateTime? time = null)
		{
			SetMe.Amount = amount;
			SetMe.User = user;
			SetMe.Channel = channel;
			SetMe.Time = time;
		}
	}

	private class CommandsGroupThrow : CommandGroup<IContext>
	{
		[Command(nameof(ShouldThrow))]
		[GenerateSwappedArguments]
		public void ShouldThrow(
			int amount,
			[Swappable]
			char? user = null,
			[Swappable]
			bool? channel = null,
			[Swappable]
			[Remainder]
			DateTime? time = null)
		{
		}
	}

	private class SetMe
	{
		public int Amount { get; set; }
		public bool? Channel { get; set; }
		public DateTime? Time { get; set; }
		public char? User { get; set; }

		public void Reset()
		{
			Amount = default;
			Channel = default;
			Time = default;
			User = default;
		}
	}
}
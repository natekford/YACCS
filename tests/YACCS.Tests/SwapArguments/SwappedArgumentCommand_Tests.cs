﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.SwapArguments;

namespace YACCS.Tests.SwapArguments
{
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

			foreach (var swapper in Swapper.CreateSwappers(new[] { 1, 2, 3 }))
			{
				var copy = args.ToArray();
				swapper.Swap(copy);

				var input = string.Join(' ', copy.Prepend(nameof(CommandsGroup.RemoveMessages)));
				var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
				Assert.IsTrue(result.InnerResult.IsSuccess);

				Assert.AreEqual(AMOUNT, setMe.Amount);
				Assert.AreEqual(USER, setMe.User);
				Assert.AreEqual(CHANNEL, setMe.Channel);
				Assert.AreEqual(TIME, setMe.Time);

				setMe.Reset();
			}
		}

		private async Task<(CommandService, SetMe, FakeContext)> CreateAsync()
		{
			var setMe = new SetMe();
			var context = new FakeContext()
			{
				Services = Utils.CreateServiceCollection().AddSingleton(setMe).BuildServiceProvider(),
			};

			var commandService = context.Get<CommandService>();
			var commands = typeof(CommandsGroup).GetAllCommandsAsync();
			await commandService.AddRangeAsync(commands).ConfigureAwait(false);

			return (commandService, setMe, context);
		}

		private class CommandsGroup : CommandGroup<IContext>
		{
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
}
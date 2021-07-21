﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class CommandTypeReader_Tests : TypeReader_Tests<IReadOnlyCollection<IImmutableCommand>>
	{
		public override ITypeReader<IReadOnlyCollection<IImmutableCommand>> Reader { get; }
			= new CommandsTypeReader();

		[TestMethod]
		public async Task Valid_Test()
		{
			await SetupAsync().ConfigureAwait(false);
			var result = await Reader.ReadAsync(Context, new[] { FakeCommandGroup._Name }).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(IReadOnlyCollection<IImmutableCommand>));
			Assert.AreEqual(1, result.Value!.Count);
		}

		protected override Task SetupAsync()
		{
			var commandService = Context.Get<CommandService>();
			var commands = typeof(FakeCommandGroup).GetDirectCommandsAsync(Context.Services);
			return commandService.AddRangeAsync(commands);
		}

		private class FakeCommandGroup : CommandGroup<IContext>
		{
			public const string _Name = "joeba";

			[Command(_Name)]
			public void Test()
			{
			}
		}
	}
}
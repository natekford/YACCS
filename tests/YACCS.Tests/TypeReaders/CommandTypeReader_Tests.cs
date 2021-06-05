using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class CommandTypeReader_Tests : TypeReader_Tests<IReadOnlyList<IImmutableCommand>>
	{
		public override IContext Context { get; }
		public override ITypeReader<IReadOnlyList<IImmutableCommand>> Reader { get; }
			= new CommandsTypeReader();

		public CommandTypeReader_Tests()
		{
			Context = new FakeContext
			{
				Services = Utils.CreateServices(),
			};

			foreach (var command in typeof(FakeCommandGroup).GetDirectCommandsAsync().GetAwaiter().GetResult())
			{
				Context.Get<CommandService>().Commands.Add(command);
			}
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, FakeCommandGroup._Name).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(IReadOnlyCollection<IImmutableCommand>));
			Assert.AreEqual(1, result.Value!.Count);
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
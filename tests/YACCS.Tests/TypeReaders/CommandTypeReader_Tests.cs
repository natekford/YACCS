using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
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
		public override TypeReader<IReadOnlyList<IImmutableCommand>> Reader { get; }
			= new CommandsTypeReader();

		public CommandTypeReader_Tests()
		{
			var service = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			Context = new FakeContext
			{
				Services = new ServiceCollection()
					.AddSingleton<ICommandService>(service)
					.BuildServiceProvider(),
			};

			foreach (var command in typeof(FakeCommandGroup).GetDirectCommandsAsync().GetAwaiter().GetResult())
			{
				service.Commands.Add(command);
			}
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, FakeCommandGroup._Name).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(IReadOnlyCollection<IImmutableCommand>));
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
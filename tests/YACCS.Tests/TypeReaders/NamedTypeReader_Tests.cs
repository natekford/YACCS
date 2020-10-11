using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class GeneratedNamedArguments_Tests
	{
		[TestMethod]
		public async Task GenerateNamedArgumentVersionAllArgs_Test()
		{
			var (commandService, setMe, context) = Create();

			var tcs = new TaskCompletionSource();
			commandService.CommandExecuted += (e) =>
			{
				tcs.SetResult();
				return Task.CompletedTask;
			};

			const int I = 1;
			const double D = 2.2;
			const string S = "three";
			var input = nameof(CommandsGroup.Test) +
				$" {CommandsGroup.I}: {I}" +
				$" {CommandsGroup.S}: {S}" +
				$" {CommandsGroup.D}: {D}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);

			await tcs.Task.ConfigureAwait(false);

			Assert.AreEqual(I, setMe.IntValue);
			Assert.AreEqual(D, setMe.DoubleValue);
			Assert.AreEqual(S, setMe.StringValue);
		}

		[TestMethod]
		public async Task GenerateNamedArgumentVersionMissingOneArg_Test()
		{
			var (commandService, setMe, context) = Create();

			var tcs = new TaskCompletionSource();
			commandService.CommandExecuted += (e) =>
			{
				tcs.SetResult();
				return Task.CompletedTask;
			};

			const int I = 1;
			const double D = 2.2;
			var input = nameof(CommandsGroup.Test) +
				$" {CommandsGroup.I}: {I}" +
				$" {CommandsGroup.D}: {D}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);

			await tcs.Task.ConfigureAwait(false);

			Assert.AreEqual(I, setMe.IntValue);
			Assert.AreEqual(D, setMe.DoubleValue);
			Assert.AreEqual(CommandsGroup.S_DEFAULT, setMe.StringValue);
		}

		private (CommandService, SetMe, FakeContext) Create()
		{
			var typeReaders = new TypeReaderRegistry();
			var commandService = new CommandService(CommandServiceConfig.Default, typeReaders);
			var command = typeof(CommandsGroup).GetDirectCommandsMutable().Single().ToCommand();
			commandService.Add(command);

			var setMe = new SetMe();
			var context = new FakeContext()
			{
				Services = new ServiceCollection()
					.AddSingleton<ITypeReaderRegistry>(typeReaders)
					.AddSingleton(setMe)
					.BuildServiceProvider(),
			};
			return (commandService, setMe, context);
		}

		private class CommandsGroup : CommandGroup<IContext>
		{
			public const string D = "val_d";
			public const string I = "val_i";
			public const string S = "val_s";
			public const string S_DEFAULT = "73 xd";

			[Command(nameof(Test))]
			[GenerateNamedArguments]
			public async Task<IResult> Test(
				[Name(D)]
				double d,
				[Name(I)]
				int i,
				[Name(S)]
				string s = S_DEFAULT)
			{
				await Task.Delay(50).ConfigureAwait(false);

				var setMe = Context.Services.GetRequiredService<SetMe>();
				setMe.DoubleValue = d;
				setMe.IntValue = i;
				setMe.StringValue = s;

				return SuccessResult.Instance.Sync;
			}
		}

		private class SetMe
		{
			public double DoubleValue { get; set; }
			public int IntValue { get; set; }
			public string StringValue { get; set; } = null!;
		}
	}

	[TestClass]
	public class NamedTypeReader_Tests : TypeReader_Tests<NamedTypeReader_Tests.NamedClass>
	{
		public override TypeReader<NamedClass> Reader { get; } = NamedTypeReaderUtils.Create<NamedClass>();

		[TestMethod]
		public async Task ReadAsync_Test()
		{
			const int NUM = 1;
			const string STR = "joe";
			var input = $"{nameof(NamedClass.Number)}: {NUM} {nameof(NamedClass.String)}: {STR}";

			var context = Create();
			var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			if (result.Value is null)
			{
				Assert.Fail();
				return;
			}
			Assert.AreEqual(NUM, result.Value.Number);
			Assert.AreEqual(STR, result.Value.String);
		}

		private FakeContext Create()
		{
			return new FakeContext()
			{
				Services = new ServiceCollection()
					.AddSingleton<ITypeReaderRegistry, TypeReaderRegistry>()
					.BuildServiceProvider(),
			};
		}

		public class NamedClass
		{
			public int Number { get; set; }
			public string String { get; set; } = "";
		}
	}
}
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.NamedArguments;
using YACCS.ParameterPreconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class GeneratedNamedArguments_Tests
	{
		private const double D = 2.2;
		private const int I = 1;
		private const string S = "three";

		[TestMethod]
		public async Task Class_Test()
		{
			var (commandService, setMe, context) = Create();

			var input = nameof(CommandsGroup.Test2) +
				$" {CommandsGroup.I}: {I}" +
				$" {CommandsGroup.S}: {S}" +
				$" {CommandsGroup.D}: {D}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);

			Assert.AreEqual(I, setMe.IntValue);
			Assert.AreEqual(D, setMe.DoubleValue);
			Assert.AreEqual(S, setMe.StringValue);
		}

		[TestMethod]
		public async Task ClassInvalidValue_Test()
		{
			var (commandService, setMe, context) = Create();

			var input = nameof(CommandsGroup.Test2) +
				$" {CommandsGroup.I}: -1" +
				$" {CommandsGroup.S}: {S}" +
				$" {CommandsGroup.D}: {D}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(InvalidParameterResult));
		}

		[TestMethod]
		public async Task ClassUnparsableValue_Test()
		{
			var (commandService, setMe, context) = Create();

			var input = nameof(CommandsGroup.Test2) +
				$" {CommandsGroup.I}: asdf" +
				$" {CommandsGroup.S}: {S}" +
				$" {CommandsGroup.D}: {D}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(TypeReaderResult<NamedArgs>));
		}

		[TestMethod]
		public async Task ParameterAllArgs_Test()
		{
			var (commandService, setMe, context) = Create();

			var tcs = new TaskCompletionSource();
			commandService.CommandExecuted += (e) =>
			{
				tcs.SetResult();
				return Task.CompletedTask;
			};

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
		public async Task ParameterMissingOneArg_Test()
		{
			var (commandService, setMe, context) = Create();

			var tcs = new TaskCompletionSource();
			commandService.CommandExecuted += (e) =>
			{
				tcs.SetResult();
				return Task.CompletedTask;
			};

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

		[TestMethod]
		public async Task ParameterWithoutDefaultValue_Test()
		{
			var (commandService, setMe, context) = Create();

			const string INPUT = nameof(CommandsGroup.Test3);
			var result = await commandService.ExecuteAsync(context, INPUT).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(NotEnoughArgsResult));
		}

		private (CommandService, SetMe, FakeContext) Create()
		{
			var typeReaders = new TypeReaderRegistry();
			var commandService = new CommandService(CommandServiceConfig.Default, typeReaders);
			foreach (var command in typeof(CommandsGroup).GetDirectCommandsMutable()
				.SelectMany(x => x.ToImmutable()))
			{
				commandService.Add(command);
			}

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

			[Command(nameof(Test2))]
			public void Test2(NamedArgs @class)
			{
				var setMe = Context.Services.GetRequiredService<SetMe>();
				setMe.DoubleValue = @class.D;
				setMe.IntValue = @class.I;
				setMe.StringValue = @class.S;
			}

			[Command(nameof(Test3))]
			[GenerateNamedArguments]
			public void Test3([Name(D)] double d)
			{
				var setMe = Context.Services.GetRequiredService<SetMe>();
				setMe.DoubleValue = d;
			}
		}

		[GenerateNamedArguments]
		private class NamedArgs
		{
			[Name(CommandsGroup.D)]
			public double D { get; set; }
			[Name(CommandsGroup.I)]
			[NotNegative]
			public int I { get; set; }
			[Name(CommandsGroup.S)]
			public string S { get; set; } = "";
		}

		private class NotNegative : ParameterPreconditionAttribute
		{
			public override Task<IResult> CheckAsync(ParameterInfo parameter, IContext context, object? value)
			{
				return this.CheckAsync<IContext, int>(parameter, context, value, (p, c, v) =>
				{
					if (v > -1)
					{
						return SuccessResult.Instance.Task;
					}
					return InvalidParameterResult.Instance.Task;
				});
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
		public override TypeReader<NamedClass> Reader { get; }
			= new NamedArgumentTypeReader<NamedClass>();

		[TestMethod]
		public async Task DuplicateKey_Test()
		{
			const int NUM = 1;
			const string STR = "joe";
			var input = $"{nameof(NamedClass.Number)}: {NUM}" +
				$" {nameof(NamedClass.String)}: {STR}" +
				$" {nameof(NamedClass.String)}: {STR}";

			var context = Create();
			var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvalidKey_Test()
		{
			const int NUM = 1;
			const string STR = "joe";
			var input = $"{nameof(NamedClass.Number)}: {NUM}" +
				$" {nameof(NamedClass.String)}: {STR}" +
				$" test: {STR}";

			var context = Create();
			var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Success_Test()
		{
			const int NUM = 1;
			const string STR = "joe";
			var input = $"{nameof(NamedClass.Number)}: {NUM}" +
				$" {nameof(NamedClass.String)}: {STR}" +
				$" {nameof(NamedClass.FieldString)}: {STR}";

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
			Assert.AreEqual(STR, result.Value.FieldString);
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
			public string FieldString = "";
			public int Number { get; set; }
			public string String { get; set; } = "";
		}
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandService_AllPreconditions_Tests
	{
		private const int DISALLOWED_VALUE = 1;

		[TestMethod]
		public async Task FailedDefaultValue_Test()
		{
			var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
			var result = await commandService.ProcessAllPreconditionsAsync(
				new PreconditionCache(context),
				command.ToCommand(),
				context,
				new[] { DISALLOWED_VALUE.ToString() },
				1
			).ConfigureAwait(false);

			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.AreEqual(CommandStage.FailedTypeReader, result.Stage);
			Assert.AreEqual(0, result.Score);

			Assert.IsTrue(command.Get<WasIReachedPrecondition>().Single().IWasReached);
			Assert.IsFalse(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task FailedParameterPrecondition_Test()
		{
			var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
			var result = await commandService.ProcessAllPreconditionsAsync(
				new PreconditionCache(context),
				command.ToCommand(),
				context,
				new[] { DISALLOWED_VALUE.ToString() },
				0
			).ConfigureAwait(false);

			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.AreEqual(CommandStage.FailedParameterPrecondition, result.Stage);
			Assert.AreEqual(0, result.Score);

			Assert.IsTrue(command.Get<WasIReachedPrecondition>().Single().IWasReached);
			Assert.IsFalse(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task FailedPrecondition_Test()
		{
			var (commandService, context, command, parameter) = Create(false, DISALLOWED_VALUE);
			var result = await commandService.ProcessAllPreconditionsAsync(
				new PreconditionCache(context),
				command.ToCommand(),
				context,
				new[] { (DISALLOWED_VALUE + 1).ToString() },
				0
			).ConfigureAwait(false);

			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.AreEqual(CommandStage.FailedPrecondition, result.Stage);
			Assert.AreEqual(0, result.Score);

			Assert.IsFalse(command.Get<WasIReachedPrecondition>().Single().IWasReached);
			Assert.IsFalse(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task FailedTypeReader_Test()
		{
			var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
			var result = await commandService.ProcessAllPreconditionsAsync(
				new PreconditionCache(context),
				command.ToCommand(),
				context,
				new[] { "joeba" },
				0
			).ConfigureAwait(false);

			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.AreEqual(CommandStage.FailedTypeReader, result.Stage);
			Assert.AreEqual(0, result.Score);

			Assert.IsTrue(command.Get<WasIReachedPrecondition>().Single().IWasReached);
			Assert.IsFalse(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task Multiple_Test()
		{
			var (commandService, context, _, _) = Create(true, DISALLOWED_VALUE);
			var commands = await typeof(CommandsGroup).CreateCommandsAsync().ConfigureAwait(false);
			var scored = commands.Select(x => CommandScore.FromCorrectArgCount(x, context, 0)).ToArray();

			var c1 = commands.ById(CommandsGroup._1).Single();
			var c2 = commands.ById(CommandsGroup._2).Single();
			Assert.AreEqual(c1, scored[0].Command);
			Assert.AreEqual(c2, scored[1].Command);

			var @checked = await commandService.ProcessAllPreconditionsAsync(
				scored,
				context,
				new[] { (DISALLOWED_VALUE + 1).ToString() }
			).ConfigureAwait(false);
			Assert.AreEqual(c2, @checked[0].Command);
			Assert.AreEqual(c1, @checked[1].Command);

			foreach (var result in @checked)
			{
				Assert.IsTrue(result.InnerResult.IsSuccess);
				Assert.AreEqual(CommandStage.CanExecute, result.Stage);
				Assert.AreEqual(int.MaxValue, result.Score);
			}
		}

		[TestMethod]
		public async Task MultipleInvalidContext_Test()
		{
			var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
			await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			{
				var scored = new List<CommandScore>
				{
					CommandScore.FromCorrectArgCount(command.ToCommand(), context, 0),
				};
				var result = await commandService.ProcessAllPreconditionsAsync(
					scored,
					new InvalidContext(),
					Array.Empty<string>()
				).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		[TestMethod]
		public async Task MultipleInvalidStage_Test()
		{
			var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
			await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			{
				var scored = new List<CommandScore>
				{
					CommandScore.FromInvalidContext(command.ToCommand(), context, 0),
				};
				var result = await commandService.ProcessAllPreconditionsAsync(
					scored,
					context,
					Array.Empty<string>()
				).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		[TestMethod]
		public async Task MultipleNullCommand_Test()
		{
			var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
			await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			{
				var scored = new List<CommandScore>
				{
					CommandScore.FromInvalidContext(null!, context, 0),
				};
				var result = await commandService.ProcessAllPreconditionsAsync(
					scored,
					context,
					Array.Empty<string>()
				).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		[TestMethod]
		public async Task Successful_Test()
		{
			var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
			var result = await commandService.ProcessAllPreconditionsAsync(
				new PreconditionCache(context),
				command.ToCommand(),
				context,
				new[] { (DISALLOWED_VALUE + 1).ToString() },
				0
			).ConfigureAwait(false);

			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.AreEqual(CommandStage.CanExecute, result.Stage);
			Assert.AreEqual(int.MaxValue, result.Score);

			Assert.IsTrue(command.Get<WasIReachedPrecondition>().Single().IWasReached);
			Assert.IsTrue(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		private (CommandService, FakeContext, ICommand, IParameter) Create(bool success, int disallowedValue)
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();

			var command = FakeDelegateCommand.New(typeof(FakeContext))
				.AsContext<FakeContext>()
				.AddPrecondition(new FakePrecondition(success))
				.AddPrecondition(new WasIReachedPrecondition());

			var parameter = new Parameter(typeof(int), "")
				.AsType<int>()
				.AddParameterPrecondition(new FakeParameterPrecondition(disallowedValue))
				.AddParameterPrecondition(new WasIReachedParameterPrecondition());
			command.Parameters.Add(parameter);

			return (commandService, context, command, parameter);
		}

		private class CommandsGroup : CommandGroup<IContext>
		{
			public const string _1 = "c1_id";
			public const string _2 = "c2_id";

			[Command]
			[Id(_1)]
			public void CommandOne()
			{
			}

			[Command]
			[Id(_2)]
			[YACCS.Commands.Attributes.Priority(1000)]
			public void CommandTwo()
			{
			}
		}

		private class FakeParameterPrecondition : ParameterPrecondition<FakeContext, int>
		{
			public int DisallowedValue { get; }

			public FakeParameterPrecondition(int value)
			{
				DisallowedValue = value;
			}

			public override Task<IResult> CheckAsync(ParameterInfo parameter, FakeContext context, [MaybeNull] int value)
				=> value == DisallowedValue ? Result.FromError("lol").AsTask() : SuccessResult.InstanceTask;
		}

		private class FakePrecondition : Precondition<FakeContext>
		{
			private readonly bool _Success;

			public FakePrecondition(bool success)
			{
				_Success = success;
			}

			public override Task<IResult> CheckAsync(IImmutableCommand command, FakeContext context)
				=> _Success ? SuccessResult.InstanceTask : Result.FromError("lol").AsTask();
		}

		private class InvalidContext : IContext
		{
			public Guid Id { get; set; }
			public IServiceProvider Services { get; set; } = EmptyServiceProvider.Instance;
		}

		private class WasIReachedParameterPrecondition : ParameterPrecondition<FakeContext, int>
		{
			public bool IWasReached { get; private set; }

			public override Task<IResult> CheckAsync(ParameterInfo parameter, FakeContext context, [MaybeNull] int value)
			{
				IWasReached = true;
				return SuccessResult.InstanceTask;
			}
		}

		private class WasIReachedPrecondition : Precondition<FakeContext>
		{
			public bool IWasReached { get; private set; }

			public override Task<IResult> CheckAsync(IImmutableCommand command, FakeContext context)
			{
				IWasReached = true;
				return SuccessResult.InstanceTask;
			}
		}
	}

	[TestClass]
	public class CommandService_Commands_Tests
	{
		[TestMethod]
		public void AddAndRemove_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			Assert.AreEqual(0, commandService.Commands.Count);

			var c1 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "1" }))
				.ToCommand();
			commandService.Add(c1);
			Assert.AreEqual(1, commandService.Commands.Count);

			var c2 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "2" }))
				.ToCommand();
			commandService.Add(c2);
			Assert.AreEqual(2, commandService.Commands.Count);

			commandService.Remove(c1);
			Assert.AreEqual(1, commandService.Commands.Count);

			commandService.Remove(c2);
			Assert.AreEqual(0, commandService.Commands.Count);
		}

		[TestMethod]
		public void AddWithParameters_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());

			var d1 = (Action<Fake>)(x => { });
			var c1 = new DelegateCommand(d1, new[] { new Name(new[] { "1" }) });
			Assert.ThrowsException<ArgumentException>(() =>
			{
				commandService.Add(c1.ToCommand());
			});

			c1.Parameters[0].OverriddenTypeReader = new TryParseTypeReader<Fake>((string input, out Fake output) =>
			{
				output = null!;
				return false;
			});
			commandService.Add(c1.ToCommand());
			Assert.AreEqual(1, commandService.Commands.Count);
		}

		[TestMethod]
		public void Find_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());

			var c1 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "1" }))
				.ToCommand();
			commandService.Add(c1);
			var c2 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "2" }))
				.AddName(new Name(new[] { "3" }))
				.ToCommand();
			commandService.Add(c2);
			var c3 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "4", "1" }))
				.AddName(new Name(new[] { "4", "2" }))
				.AddName(new Name(new[] { "4", "3" }))
				.ToCommand();
			commandService.Add(c3);
			var c4 = FakeDelegateCommand.New()
				.AddName(new Name(new[] { "4", "1" }))
				.ToCommand();
			commandService.Add(c4);

			{
				var found = commandService.Find("");
				Assert.AreEqual(0, found.Count);
			}

			{
				var found = commandService.Find("\"1");
				Assert.AreEqual(0, found.Count);
			}

			{
				var found = commandService.Find("1");
				Assert.AreEqual(1, found.Count);
				Assert.AreSame(c1, found[0]);
			}

			{
				var found = commandService.Find("2");
				Assert.AreEqual(1, found.Count);
				Assert.AreSame(c2, found[0]);
			}

			{
				var found = commandService.Find("3");
				Assert.AreEqual(1, found.Count);
				Assert.AreSame(c2, found[0]);
			}

			{
				var found = commandService.Find("4");
				Assert.AreEqual(2, found.Count);
				Assert.AreSame(c3, found[0]);
				Assert.AreSame(c4, found[1]);
			}

			{
				var found = commandService.Find("4 1");
				Assert.AreEqual(2, found.Count);
				Assert.AreSame(c3, found[0]);
				Assert.AreSame(c4, found[1]);
			}

			{
				var found = commandService.Find("4 2");
				Assert.AreEqual(1, found.Count);
				Assert.AreSame(c3, found[0]);
			}
		}

		private class Fake
		{
		}
	}

	[TestClass]
	public class CommandService_GetPotentiallyExecutableCommands_Tests
	{
		[TestMethod]
		public async Task EmptyInput_Test()
		{
			var (commandService, context) = await CreateAsync().ConfigureAwait(false);
			var commands = commandService.GetPotentiallyExecutableCommands(context, Array.Empty<string>());
			Assert.AreEqual(0, commands.Count);
		}

		[TestMethod]
		public async Task InvalidContext_Type()
		{
			var (commandService, context) = await CreateAsync().ConfigureAwait(false);
			var input = new[] { CommandGroup._NAME };

			{
				var commands = commandService.GetPotentiallyExecutableCommands(context, input);
				Assert.AreEqual(4, commands.Count(x => x.Stage != CommandStage.BadContext));
			}

			var c1 = FakeDelegateCommand.New(typeof(FakeContext2))
				.AddName(new Name(new[] { CommandGroup._NAME }))
				.ToCommand();
			commandService.Add(c1);

			{
				var commands = commandService.GetPotentiallyExecutableCommands(new FakeContext2(), input);
				Assert.AreEqual(5, commands.Count(x => x.Stage != CommandStage.BadContext));
			}

			{
				var commands = commandService.GetPotentiallyExecutableCommands(context, input);
				Assert.AreEqual(4, commands.Count(x => x.Stage != CommandStage.BadContext));
			}
		}

		[TestMethod]
		public async Task Length_Test()
		{
			var (commandService, context) = await CreateAsync().ConfigureAwait(false);

			{
				var commands = commandService.GetPotentiallyExecutableCommands(context, new[] { CommandGroup._NAME });
				Assert.AreEqual(2, commands.Count(x => x.Stage == CommandStage.CorrectArgCount));
			}

			{
				var commands = commandService.GetPotentiallyExecutableCommands(context, new[] { CommandGroup._NAME, "a" });
				Assert.AreEqual(2, commands.Count(x => x.Stage == CommandStage.CorrectArgCount));
			}

			{
				var commands = commandService.GetPotentiallyExecutableCommands(context, new[] { CommandGroup._NAME, "a", "b" });
				Assert.AreEqual(2, commands.Count(x => x.Stage == CommandStage.CorrectArgCount));
			}

			{
				var commands = commandService.GetPotentiallyExecutableCommands(context, new[] { CommandGroup._NAME, "a", "b", "c" });
				Assert.AreEqual(1, commands.Count(x => x.Stage == CommandStage.CorrectArgCount));
			}
		}

		private async Task<(CommandService, FakeContext)> CreateAsync()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			await commandService.AddAsync(typeof(CommandGroup).GetCommandsAsync()).ConfigureAwait(false);
			var context = new FakeContext();
			return (commandService, context);
		}

		[Command(_NAME)]
		private class CommandGroup : CommandGroup<FakeContext>
		{
			public const string _NAME = "joe";

			[Command]
			public void TestCommand()
			{
			}

			[Command]
			public void TestCommand(string input)
			{
			}

			[Command]
			public void TestCommand([Count(2)] IEnumerable<string> input)
			{
			}

			[Command]
			[YACCS.Commands.Attributes.Priority(100)]
			public void TestCommandWithRemainder([Remainder] string input)
			{
			}
		}

		private class FakeContext2 : FakeContext
		{
		}
	}

	[TestClass]
	public class CommandService_ParameterPreconditions_Tests
	{
		[TestMethod]
		public async Task EnumerableFailure_Test()
		{
			const int DISALLOWED_VALUE = 1;

			var (commandService, context, command, parameter) = Create(DISALLOWED_VALUE);
			var result = await commandService.ProcessParameterPreconditionsAsync(
				new PreconditionCache(context),
				command,
				parameter,
				new[] { DISALLOWED_VALUE, DISALLOWED_VALUE + 1, DISALLOWED_VALUE + 2 }
			).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsFalse(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task EnumerableSuccess_Test()
		{
			const int DISALLOWED_VALUE = 1;

			var (commandService, context, command, parameter) = Create(DISALLOWED_VALUE);
			var result = await commandService.ProcessParameterPreconditionsAsync(
				new PreconditionCache(context),
				command,
				parameter,
				new[] { DISALLOWED_VALUE + 1, DISALLOWED_VALUE + 2, DISALLOWED_VALUE + 3 }
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsTrue(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task NonEnumerableFailure_Test()
		{
			const int DISALLOWED_VALUE = 1;

			var (commandService, context, command, parameter) = Create(DISALLOWED_VALUE);
			var result = await commandService.ProcessParameterPreconditionsAsync(
				new PreconditionCache(context),
				command,
				parameter,
				DISALLOWED_VALUE
			).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsFalse(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task NonEnumerableSuccess_Test()
		{
			const int DISALLOWED_VALUE = 1;

			var (commandService, context, command, parameter) = Create(DISALLOWED_VALUE);
			var result = await commandService.ProcessParameterPreconditionsAsync(
				new PreconditionCache(context),
				command,
				parameter,
				DISALLOWED_VALUE + 1
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsTrue(parameter.Get<WasIReachedParameterPrecondition>().Single().IWasReached);
		}

		private (CommandService, FakeContext, IImmutableCommand, IImmutableParameter) Create(int disallowedValue)
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameterBuilder = new Parameter(typeof(int), "")
				.AsType<int>()
				.AddParameterPrecondition(new FakeParameterPrecondition(disallowedValue))
				.AddParameterPrecondition(new WasIReachedParameterPrecondition());
			var commandBuilder = FakeDelegateCommand.New();
			commandBuilder.Parameters.Add(parameterBuilder);
			var command = commandBuilder.ToCommand();
			var parameter = parameterBuilder.ToParameter();
			return (commandService, context, command, parameter);
		}

		private class FakeParameterPrecondition : ParameterPrecondition<IContext, int>
		{
			public int DisallowedValue { get; }

			public FakeParameterPrecondition(int value)
			{
				DisallowedValue = value;
			}

			public override Task<IResult> CheckAsync(ParameterInfo parameter, IContext context, [MaybeNull] int value)
				=> value == DisallowedValue ? Result.FromError("lol").AsTask() : SuccessResult.InstanceTask;
		}

		private class WasIReachedParameterPrecondition : ParameterPrecondition<IContext, int>
		{
			public bool IWasReached { get; private set; }

			public override Task<IResult> CheckAsync(ParameterInfo parameter, IContext context, [MaybeNull] int value)
			{
				IWasReached = true;
				return SuccessResult.InstanceTask;
			}
		}
	}

	[TestClass]
	public class CommandService_Preconditions_Tests
	{
		[TestMethod]
		public async Task ProcessPreconditionsFailure_Test()
		{
			var (commandService, context, command) = Create(false);
			var result = await commandService.ProcessPreconditionsAsync(
				new PreconditionCache(context),
				command
			).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsFalse(command.Get<WasIReachedPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task ProcessPreconditionsSuccess_Test()
		{
			var (commandService, context, command) = Create(true);
			var result = await commandService.ProcessPreconditionsAsync(
				new PreconditionCache(context),
				command
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsTrue(command.Get<WasIReachedPrecondition>().Single().IWasReached);
		}

		private (CommandService, FakeContext, IImmutableCommand) Create(bool success)
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var command = FakeDelegateCommand.New()
				.AsContext<IContext>()
				.AddPrecondition(new FakePrecondition(success))
				.AddPrecondition(new WasIReachedPrecondition())
				.ToCommand();
			return (commandService, context, command);
		}

		private class FakePrecondition : Precondition<IContext>
		{
			private readonly bool _Success;

			public FakePrecondition(bool success)
			{
				_Success = success;
			}

			public override Task<IResult> CheckAsync(IImmutableCommand command, IContext context)
				=> _Success ? SuccessResult.InstanceTask : Result.FromError("lol").AsTask();
		}

		private class WasIReachedPrecondition : Precondition<IContext>
		{
			public bool IWasReached { get; private set; }

			public override Task<IResult> CheckAsync(IImmutableCommand command, IContext context)
			{
				IWasReached = true;
				return SuccessResult.InstanceTask;
			}
		}
	}

	[TestClass]
	public class CommandService_TypeReaders_Tests
	{
		[TestMethod]
		public async Task ProcessTypeReaderMultipleButNotAllValues_Test()
		{
			var value = new[] { 1, 2, 3, 4 };
			var (commandService, context, parameter) = Create<int[]>(4);
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				value.Select(x => x.ToString()).Append("joeba").Append("trash").ToArray(),
				0
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, value.GetType());

			var cast = (int[])result.Arg!;
			for (var i = 0; i < value.Length; ++i)
			{
				Assert.AreEqual(value[i], cast[i]);
			}
		}

		[TestMethod]
		public async Task ProcessTypeReaderMultipleValuesAllValues_Test()
		{
			var value = new[] { 1, 2, 3, 4 };
			var (commandService, context, parameter) = Create<int[]>(4);
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				value.Select(x => x.ToString()).ToArray(),
				0
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, value.GetType());

			var cast = (int[])result.Arg!;
			for (var i = 0; i < value.Length; ++i)
			{
				Assert.AreEqual(value[i], cast[i]);
			}
		}

		[TestMethod]
		public async Task ProcessTypeReaderMultipleValuesLongerThanArgs_Test()
		{
			var value = new[] { 1, 2, 3, 4 };
			var (commandService, context, parameter) = Create<int[]>(null);
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				value.Select(x => x.ToString()).ToArray(),
				0
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, value.GetType());

			var cast = (int[])result.Arg!;
			for (var i = 0; i < value.Length; ++i)
			{
				Assert.AreEqual(value[i], cast[i]);
			}
		}

		[TestMethod]
		public async Task ProcessTypeReaderNotRegistered_Test()
		{
			var (commandService, context, parameter) = Create<CommandService_TypeReaders_Tests>(1);
			await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			{
				var result = await commandService.ProcessTypeReadersAsync(
					new PreconditionCache(context),
					parameter.ToParameter(),
					new[] { "joeba" },
					0
				).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		[TestMethod]
		public async Task ProcessTypeReaderOverridden_Test()
		{
			var (commandService, context, parameter) = Create<char>(1);
			parameter.OverriddenTypeReader = new CoolCharTypeReader();
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				new[] { "joeba" },
				0
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task ProcessTypeReadersCharFailure_Test()
		{
			var (commandService, context, parameter) = Create<char>(1);
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				new[] { "joeba" },
				0
			).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task ProcessTypeReaderSingleValue_Test()
		{
			const int VALUE = 2;
			var (commandService, context, parameter) = Create<int>(1);
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				new[] { VALUE.ToString() },
				0
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, VALUE.GetType());
			Assert.AreEqual(VALUE, result.Arg);
		}

		[TestMethod]
		public async Task ProcessTypeReaderSingleValueWhenMultipleExist_Test()
		{
			const int VALUE = 2;
			var (commandService, context, parameter) = Create<int>(1);
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				new[] { VALUE.ToString(), "joeba", "trash" },
				0
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, VALUE.GetType());
			Assert.AreEqual(VALUE, result.Arg);
		}

		[TestMethod]
		public async Task ProcessTypeReadersString_Test()
		{
			const string VALUE = "joeba";
			var (commandService, context, parameter) = Create<string>(1);
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				new[] { VALUE },
				0
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, VALUE.GetType());
			Assert.AreEqual(VALUE, result.Arg);
		}

		[TestMethod]
		public async Task ProcessTypeReaderZeroLength_Test()
		{
			var (commandService, context, parameter) = Create<IContext>(0);
			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter.ToParameter(),
				new[] { "doesn't matter" },
				0
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, typeof(IContext));
		}

		private (CommandService, FakeContext, IParameter) Create<T>(int? length)
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameter = new Parameter(typeof(T), "")
			{
				Attributes = new List<object>
				{
					new CountAttribute(length),
				},
			};
			return (commandService, context, parameter);
		}

		private class CoolCharTypeReader : TypeReader<char>
		{
			public override Task<ITypeReaderResult<char>> ReadAsync(IContext context, string input)
				=> TypeReaderResult<char>.FromSuccess('z').AsTask();
		}

		private class FailOnQTypeReader : TypeReader<char>
		{
			public override Task<ITypeReaderResult<char>> ReadAsync(IContext context, string input)
			{
				return input.Equals("q", StringComparison.OrdinalIgnoreCase)
					? TypeReaderResult<char>.FailureTask
					: TypeReaderResult<char>.FromSuccess('z').AsTask();
			}
		}
	}
}
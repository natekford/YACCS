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
	public class CommandService_AllPrecondition_Test
	{
		[TestMethod]
		public async Task FailedDefaultValue_Test()
		{
			const int DISALLOWED_VALUE = 1;

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
			const int DISALLOWED_VALUE = 1;

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
			const int DISALLOWED_VALUE = 1;

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
			const int DISALLOWED_VALUE = 1;

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
		public async Task Successful_Test()
		{
			const int DISALLOWED_VALUE = 1;

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

			var command = FakeDelegateCommand.New
				.AsContext<IContext>()
				.AddPrecondition(new FakePrecondition(success))
				.AddPrecondition(new WasIReachedPrecondition());

			var parameter = new Parameter(typeof(int), "")
				.AsType<int>()
				.AddParameterPrecondition(new FakeParameterPrecondition(disallowedValue))
				.AddParameterPrecondition(new WasIReachedParameterPrecondition());
			command.Parameters.Add(parameter);

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

		private class WasIReachedParameterPrecondition : ParameterPrecondition<IContext, int>
		{
			public bool IWasReached { get; private set; }

			public override Task<IResult> CheckAsync(ParameterInfo parameter, IContext context, [MaybeNull] int value)
			{
				IWasReached = true;
				return SuccessResult.InstanceTask;
			}
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
	public class CommandService_ParameterPrecondition_Tests
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
			var commandBuilder = FakeDelegateCommand.New;
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
	public class CommandService_Precondition_Tests
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
			var command = FakeDelegateCommand.New
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
	public class CommandService_TypeReader_Tests
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
			var (commandService, context, parameter) = Create<int[]>(500);
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
			var (commandService, context, parameter) = Create<IDictionary<ArgumentNullException, IDictionary<string, char>>>(1);
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

		private (CommandService, FakeContext, IParameter) Create<T>(int length)
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameter = new Parameter(typeof(T), "")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(length),
				},
			};
			return (commandService, context, parameter);
		}

		private class CoolCharTypeReader : TypeReader<char>
		{
			public override Task<ITypeReaderResult<char>> ReadAsync(IContext context, string input)
				=> TypeReaderResult<char>.FromSuccess('z').AsTask();
		}
	}
}
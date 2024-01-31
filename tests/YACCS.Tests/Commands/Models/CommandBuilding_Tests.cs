using Microsoft.VisualStudio.TestTools.UnitTesting;

using MorseCode.ITask;

using System;
using System.Runtime.CompilerServices;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands.Models;

[TestClass]
public class CommandBuilding_Tests
{
	private static string[][] Paths { get; } = [["Joe"]];

	[TestMethod]
	public async Task CommandBuildingThrowWhenGenericTypeDefinitions_Test()
	{
		var declaredCommands = typeof(GroupGeneric<object>).CreateMutableCommands();
		Assert.AreEqual(1, declaredCommands.Count);

		var concreteType = 0;
		await foreach (var actualCommand in typeof(GroupGeneric<object>).GetDirectCommandsAsync(EmptyServiceProvider.Instance))
		{
			++concreteType;
		}
		Assert.AreEqual(1, concreteType);

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
		{
			await foreach (var actualCommand in typeof(GroupGeneric<>).GetDirectCommandsAsync(EmptyServiceProvider.Instance))
			{
			}
		}).ConfigureAwait(false);
	}

	[TestMethod]
	public void CommandBuildingThrowWhenStructs_Test()
	{
		Assert.ThrowsException<ArgumentException>(static () =>
		{
			_ = typeof(GroupStruct).CreateMutableCommands();
		});
	}

	[TestMethod]
	public async Task CommandDelegateBuilding_Test()
	{
		var command = new DelegateCommand((IContext arg) => true, Paths);
		Assert.AreEqual(Paths.Length, command.Paths.Count);
		Assert.AreEqual(Paths[0], command.Paths[0]);
		Assert.AreEqual(1, command.Parameters.Count);
		Assert.AreEqual(1, command.Attributes.Count);
		Assert.IsInstanceOfType(command.Attributes[0], typeof(Delegate));

		var immutable = command.ToImmutable();
		var args = new object[] { new FakeContext() };
		var result = await immutable.ExecuteAsync(null!, args).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result, typeof(ValueResult));
		Assert.IsTrue(result.TryGetValue(out bool value));
		Assert.AreEqual(true, value);
	}

	[TestMethod]
	public async Task CommandMethodInfoBuilding_Test()
	{
		var commands = new List<IImmutableCommand>();
		await foreach (var (_, command) in typeof(GroupBase).GetAllCommandsAsync(EmptyServiceProvider.Instance))
		{
			commands.Add(command);
		}
		Assert.AreEqual(2, commands.Count);

		var command1 = commands.ById(GroupBase.INHERITANCE_ALLOWED).SingleOrDefault();
		Assert.IsNotNull(command1);

		var command2 = commands.ById(GroupBase.INHERITANCE_DISALLOWED).SingleOrDefault();
		Assert.IsNotNull(command2);
	}

	[TestMethod]
	public async Task CommandMethodInfoBuildingWithInheritanceInvolved_Test()
	{
		var commands = new List<IImmutableCommand>();
		await foreach (var (_, command) in typeof(GroupChild).GetAllCommandsAsync(EmptyServiceProvider.Instance))
		{
			commands.Add(command);
		}
		Assert.AreEqual(1, commands.Count);

		var command1 = commands.ById(GroupBase.INHERITANCE_ALLOWED).SingleOrDefault();
		Assert.IsNotNull(command1);

		var command2 = commands.ById(GroupBase.INHERITANCE_DISALLOWED).SingleOrDefault();
		Assert.IsNull(command2);
	}

	[TestMethod]
	public async Task HiddenAttribute_Test()
	{
		var commands = new List<IImmutableCommand>();
		await foreach (var (_, command) in typeof(HiddenAttributeGroup).GetAllCommandsAsync(EmptyServiceProvider.Instance))
		{
			commands.Add(command);
		}
		Assert.AreEqual(2, commands.Count);

		var hidden = commands.ById(HiddenAttributeGroup.HIDDEN).SingleOrDefault();
		Assert.IsNotNull(hidden);
		Assert.IsTrue(hidden.IsHidden);

		var visible = commands.ById(HiddenAttributeGroup.VISIBLE).SingleOrDefault();
		Assert.IsNotNull(visible);
		Assert.IsFalse(visible.IsHidden);
	}

	[TestMethod]
	public void OverriddenTypeReader_Test()
	{
		static void Delegate([FakeTypeReader] string value)
		{
		}

		var immutable = new DelegateCommand(Delegate, Paths).ToImmutable();
		Assert.AreEqual(1, immutable.Parameters.Count);
		Assert.IsInstanceOfType(immutable.Parameters[0].TypeReader, typeof(FakeTypeReader));
	}

	[TestMethod]
	public void OverriddenTypeReaderInvalidType_Test()
	{
		static void Delegate([FakeTypeReader] int value)
		{
		}

		Assert.ThrowsException<ArgumentException>(() =>
		{
			_ = new DelegateCommand(Delegate, Paths).ToImmutable();
		});
	}

	[TestMethod]
	public void RemainderAttributeInvalid_Test()
	{
		var commands = typeof(RemainderAttributeGroup).CreateMutableCommands();
		Assert.AreEqual(3, commands.Count);

		var invalid = commands.ById(RemainderAttributeGroup.INVALID_REMAINDER).SingleOrDefault();
		Assert.IsNotNull(invalid);

		Assert.ThrowsException<InvalidOperationException>(() =>
		{
			_ = invalid.ToImmutable();
		});
	}

	[TestMethod]
	public void RemainderAttributeParams_Test()
	{
		var commands = typeof(RemainderAttributeGroup).CreateMutableCommands();
		Assert.AreEqual(3, commands.Count);

		var @params = commands.ById(RemainderAttributeGroup.PARAMS).SingleOrDefault();
		Assert.IsNotNull(@params);

		var immutable = @params.ToImmutable();
		Assert.AreEqual(int.MaxValue, immutable.MaxLength);
	}

	[TestMethod]
	public void RemainderAttributeValid_Test()
	{
		var commands = typeof(RemainderAttributeGroup).CreateMutableCommands();
		Assert.AreEqual(3, commands.Count);

		var valid = commands.ById(RemainderAttributeGroup.VALID_REMAINDER).SingleOrDefault();
		Assert.IsNotNull(valid);

		var immutable = valid.ToImmutable();
		Assert.AreEqual(int.MaxValue, immutable.MaxLength);
	}

	[TestMethod]
	public async Task StaticCommandDelegateBuilding_Test()
	{
		static bool Delegate(IContext arg) => true;

		var command = new DelegateCommand(Delegate, Paths);
		Assert.AreEqual(Paths.Length, command.Paths.Count);
		Assert.AreEqual(Paths[0], command.Paths[0]);
		Assert.AreEqual(1, command.Parameters.Count);
		Assert.AreEqual(2, command.Attributes.Count);
		Assert.IsInstanceOfType(command.Attributes[0], typeof(CompilerGeneratedAttribute));
		Assert.IsInstanceOfType(command.Attributes[1], typeof(Delegate));

		var immutable = command.ToImmutable();
		var args = new object[] { new FakeContext() };
		var result = await immutable.ExecuteAsync(null!, args).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result, typeof(ValueResult));
		Assert.IsTrue(result.TryGetValue(out bool value));
		Assert.AreEqual(true, value);
	}

	private struct GroupStruct : ICommandGroup
	{
		public readonly Task AfterExecutionAsync(IImmutableCommand command, IContext context, IResult result)
			=> Task.CompletedTask;

		public readonly Task BeforeExecutionAsync(IImmutableCommand command, IContext context)
			=> Task.CompletedTask;

		[Command(nameof(Command))]
		public void Command()
		{
		}

		public readonly Task OnCommandBuildingAsync(IServiceProvider services, IList<ICommand> commands)
			=> Task.CompletedTask;
	}

	private class FakeTypeReader :
		TypeReader<IContext, string>,
		IOverrideTypeReaderAttribute
	{
		public const string VALUE = "joe";

		ITypeReader IOverrideTypeReaderAttribute.Reader => this;

		public override ITask<ITypeReaderResult<string>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> Success(VALUE).AsITask();
	}

	private class GroupBase : CommandGroup<FakeContext>
	{
		public const string INHERITANCE_ALLOWED = "id_1";
		public const string INHERITANCE_DISALLOWED = "id_2";

		[Command(nameof(InheritanceAllowed), AllowInheritance = true)]
		[Id(INHERITANCE_ALLOWED)]
		public IResult InheritanceAllowed() => Success.Instance;

		[Command(nameof(InheritanceDisallowed), AllowInheritance = false)]
		[Id(INHERITANCE_DISALLOWED)]
		public IResult InheritanceDisallowed() => Success.Instance;
	}

	private class GroupChild : GroupBase;

	private class GroupGeneric<T> : GroupBase;

	private class HiddenAttributeGroup : CommandGroup<FakeContext>
	{
		public const string HIDDEN = "hidden";
		public const string VISIBLE = "visible";

		[Command(nameof(Hidden))]
		[Id(HIDDEN)]
		[Hidden]
		public IResult Hidden() => Success.Instance;

		[Command(nameof(Visible))]
		[Id(VISIBLE)]
		public IResult Visible() => Success.Instance;
	}

	private class RemainderAttributeGroup : CommandGroup<FakeContext>
	{
		public const string INVALID_REMAINDER = "invalid";
		public const string PARAMS = "params";
		public const string VALID_REMAINDER = "valid";

		[Command(nameof(InvalidRemainder))]
		[Id(INVALID_REMAINDER)]
		public IResult InvalidRemainder([Remainder] string[] input, int a) => Success.Instance;

		[Command(nameof(Params))]
		[Id(PARAMS)]
		public IResult Params(params string[] input) => Success.Instance;

		[Command(nameof(ValidRemainder))]
		[Id(VALID_REMAINDER)]
		public IResult ValidRemainder([Remainder] string[] input) => Success.Instance;
	}
}
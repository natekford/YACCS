using Microsoft.VisualStudio.TestTools.UnitTesting;

using MorseCode.ITask;

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
	public async Task CategoryInheritance_Test()
	{
		var commands = new List<IImmutableCommand>();
		await foreach (var (_, command) in typeof(GroupCategoryChild).GetAllCommandsAsync(FakeServiceProvider.Instance))
		{
			commands.Add(command);
		}
		Assert.AreEqual(1, commands.Count);

		Assert.AreEqual(1, commands.Single().Categories.Count);
	}

	[TestMethod]
	public async Task CategorySubclass_Test()
	{
		var commands = new List<IImmutableCommand>();
		await foreach (var (_, command) in typeof(GroupCategoryParent).GetAllCommandsAsync(FakeServiceProvider.Instance))
		{
			commands.Add(command);
		}
		Assert.AreEqual(1, commands.Count);

		Assert.AreEqual(2, commands.Single().Categories.Count);
	}

	[TestMethod]
	public async Task CommandBuildingThrowWhenGenericTypeDefinitions_Test()
	{
		var declaredCommands = typeof(GroupGeneric<object>).CreateMutableCommands();
		Assert.AreEqual(1, declaredCommands.Count);

		var concreteType = 0;
		await foreach (var actualCommand in typeof(GroupGeneric<object>).GetDirectCommandsAsync(FakeServiceProvider.Instance))
		{
			++concreteType;
		}
		Assert.AreEqual(1, concreteType);

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
		{
			await foreach (var actualCommand in typeof(GroupGeneric<>).GetDirectCommandsAsync(FakeServiceProvider.Instance))
			{
			}
		}).ConfigureAwait(false);
	}

	[TestMethod]
	public void CommandBuildingThrowWhenStructs_Test()
	{
		Assert.ThrowsExactly<ArgumentException>(static () =>
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
		Assert.IsInstanceOfType<Delegate>(command.Attributes[0]);

		var immutable = command.ToImmutable();
		var args = new object[] { new FakeContext() };
		var result = await immutable.ExecuteAsync(null!, args).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType<ValueResult>(result);
		Assert.IsTrue(result.TryGetValue(out bool value));
		Assert.IsTrue(value);
	}

	[TestMethod]
	public async Task CommandMethodInfoBuilding_Test()
	{
		var commands = new List<IImmutableCommand>();
		await foreach (var (_, command) in typeof(GroupBase).GetAllCommandsAsync(FakeServiceProvider.Instance))
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
		await foreach (var (_, command) in typeof(GroupChild).GetAllCommandsAsync(FakeServiceProvider.Instance))
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
		await foreach (var (_, command) in typeof(HiddenAttributeGroup).GetAllCommandsAsync(FakeServiceProvider.Instance))
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
		Assert.IsInstanceOfType<FakeTypeReader>(immutable.Parameters[0].TypeReader);
	}

	[TestMethod]
	public void OverriddenTypeReaderInvalidType_Test()
	{
		static void Delegate([FakeTypeReader] int value)
		{
		}

		Assert.ThrowsExactly<ArgumentException>(() =>
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

		Assert.ThrowsExactly<InvalidOperationException>(() =>
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
		Assert.IsInstanceOfType<CompilerGeneratedAttribute>(command.Attributes[0]);
		Assert.IsInstanceOfType<Delegate>(command.Attributes[1]);

		var immutable = command.ToImmutable();
		var args = new object[] { new FakeContext() };
		var result = await immutable.ExecuteAsync(null!, args).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType<ValueResult>(result);
		Assert.IsTrue(result.TryGetValue(out bool value));
		Assert.IsTrue(value);
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

		public readonly Task OnCommandBuildingAsync(IServiceProvider services, IList<IMutableCommand> commands)
			=> Task.CompletedTask;
	}

	private class FakeTypeReader : TypeReader<string>, IOverrideTypeReaderAttribute
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
		public Result InheritanceAllowed() => CachedResults.Success;

		[Command(nameof(InheritanceDisallowed), AllowInheritance = false)]
		[Id(INHERITANCE_DISALLOWED)]
		public Result InheritanceDisallowed() => CachedResults.Success;
	}

	private class GroupCategoryChild : GroupCategoryParent
	{
		[Command(nameof(Foo))]
		public Result Foo() => CachedResults.Success;
	}

	[Category(nameof(GroupCategoryParent))]
	private class GroupCategoryParent : CommandGroup<FakeContext>
	{
		[Category(nameof(GroupCategorySubclass))]
		public class GroupCategorySubclass : CommandGroup<FakeContext>
		{
			[Command(nameof(Bar))]
			public Result Bar() => CachedResults.Success;
		}
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
		public Result Hidden() => CachedResults.Success;

		[Command(nameof(Visible))]
		[Id(VISIBLE)]
		public Result Visible() => CachedResults.Success;
	}

	private class RemainderAttributeGroup : CommandGroup<FakeContext>
	{
		public const string INVALID_REMAINDER = "invalid";
		public const string PARAMS = "params";
		public const string VALID_REMAINDER = "valid";

		[Command(nameof(InvalidRemainder))]
		[Id(INVALID_REMAINDER)]
		public Result InvalidRemainder([Remainder] string[] input, int a) => CachedResults.Success;

		[Command(nameof(Params))]
		[Id(PARAMS)]
		public Result Params(params string[] input) => CachedResults.Success;

		[Command(nameof(ValidRemainder))]
		[Id(VALID_REMAINDER)]
		public Result ValidRemainder([Remainder] string[] input) => CachedResults.Success;
	}
}
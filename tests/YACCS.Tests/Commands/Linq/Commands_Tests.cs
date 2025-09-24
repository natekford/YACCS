using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Commands.Linq;

[TestClass]
public class Commands_Tests
{
	private const string CHILD_ID = "child_id";
	private const string DUPE_ID = "dupe_id";
	private const string NORM_ID = "normal_id";
	private const string PARENT_ID = "parent_id";
	private readonly List<IMutableCommand> _Commands =
	[
		FakeDelegateCommand.New().AddAttribute(new IdAttribute(DUPE_ID)),
		FakeDelegateCommand.New().AddAttribute(new IdAttribute(DUPE_ID)),
		FakeDelegateCommand.New().AddAttribute(new IdAttribute(NORM_ID)),
		new ReflectionCommand(typeof(GroupBase).GetMethod(nameof(GroupBase.CommandParent))!)
		{
			Attributes = [new(new IdAttribute(PARENT_ID))],
		},
		new ReflectionCommand(typeof(GroupChild).GetMethod(nameof(GroupChild.CommandChild))!)
		{
			Attributes = [new(new IdAttribute(CHILD_ID))],
		},
	];

	[TestMethod]
	public void AddName_Test()
	{
		var command = _Commands.ById(NORM_ID).Single().AsContext<FakeContext>();
		Assert.AreEqual(0, command.Paths.Count);
		command.AddPath(["joe", "mama"]);
		Assert.AreEqual(1, command.Paths.Count);
	}

	[TestMethod]
	public void AddPrecondition_Test()
	{
		var command = _Commands.ById(NORM_ID).Single().AsContext<FakeContext>();
		Assert.AreEqual(0, command.GetAttributes<IPrecondition>().Count());
		command.AddPrecondition(new NotSevenPM());
		Assert.AreEqual(1, command.GetAttributes<IPrecondition>().Count());
	}

	[TestMethod]
	public void AsCommand_Test()
	{
		Assert.ThrowsExactly<ArgumentNullException>(static () =>
		{
			var command = default(IQueryableEntity)!.AsCommand();
		});

		Assert.ThrowsExactly<ArgumentException>(static () =>
		{
			var parameter = new Parameter(typeof(string), "joe", typeof(string));
			var command = parameter.AsCommand();
		});

		var command = _Commands[0].AsCommand();
		Assert.IsNotNull(command);
	}

	[TestMethod]
	public void AsContext_Test()
	{
		var parent = _Commands.ById(PARENT_ID).Single();
		var child = _Commands.ById(CHILD_ID).Single();

		var child_parent = parent.AsContext<FakeContextChild>();
		Assert.IsInstanceOfType<ICommand<FakeContextChild>>(child_parent);
		var child_child = child.AsContext<FakeContextChild>();
		Assert.IsInstanceOfType<ICommand<FakeContextChild>>(child_child);

		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			var parent_child = child.AsContext<FakeContext>();
		});
		var parent_parent = parent.AsContext<FakeContext>();
		Assert.IsInstanceOfType<ICommand<FakeContext>>(parent_parent);
	}

	[TestMethod]
	public void GetCommandsByType_Test()
	{
		{
			var parameters = _Commands.GetCommandsByType<FakeContext>();
			Assert.AreEqual(4, parameters.Count());
		}

		{
			var parameters = _Commands.GetCommandsByType<IContext>();
			Assert.AreEqual(3, parameters.Count());
		}

		{
			var parameters = _Commands.GetCommandsByType<OtherContext>();
			Assert.AreEqual(3, parameters.Count());
		}
	}

	private class GroupBase : CommandGroup<FakeContext>
	{
		[Command("joe")]
		public void CommandParent()
		{
		}
	}

	private class GroupChild : CommandGroup<FakeContextChild>
	{
		[Command("joe")]
		public void CommandChild()
		{
		}
	}

	private sealed class NotSevenPM : SummarizablePrecondition<FakeContext>
	{
		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			FakeContext context)
		{
			if (DateTime.UtcNow.Hour != 19)
			{
				return new(Result.EmptySuccess);
			}
			return new(Result.Failure("It's seven PM."));
		}

		public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
			=> throw new NotImplementedException();
	}
}
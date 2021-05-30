using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Commands.Linq
{
	[TestClass]
	public class Commands_Tests
	{
		private const string CHILD_ID = "child_id";
		private const string DUPE_ID = "dupe_id";
		private const string NORM_ID = "normal_id";
		private const string PARENT_ID = "parent_id";
		private readonly List<ICommand> _Commands = new()
		{
			FakeDelegateCommand.New().AddAttribute(new IdAttribute(DUPE_ID)),
			FakeDelegateCommand.New().AddAttribute(new IdAttribute(DUPE_ID)),
			FakeDelegateCommand.New().AddAttribute(new IdAttribute(NORM_ID)),
			new ReflectionCommand(typeof(GroupBase).GetMethod(nameof(GroupBase.CommandParent))!)
			{
				Attributes = new List<object>
				{
					new IdAttribute(PARENT_ID),
				},
			},
			new ReflectionCommand(typeof(GroupChild).GetMethod(nameof(GroupChild.CommandChild))!)
			{
				Attributes = new List<object>
				{
					new IdAttribute(CHILD_ID),
				},
			},
		};

		[TestMethod]
		public void AddName_Test()
		{
			var command = _Commands.GetCommandById<FakeContext>(NORM_ID);
			Assert.AreEqual(0, command.Names.Count);
			command.AddName(new[] { "joe", "mama" });
			Assert.AreEqual(1, command.Names.Count);
		}

		[TestMethod]
		public void AddPrecondition_Test()
		{
			var command = _Commands.GetCommandById<FakeContext>(NORM_ID);
			Assert.AreEqual(0, command.Get<IPrecondition>().Count());
			command.AddPrecondition(new NotSevenPM());
			Assert.AreEqual(1, command.Get<IPrecondition>().Count());
		}

		[TestMethod]
		public void AsCommand_Test()
		{
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				var command = default(IQueryableEntity)!.AsCommand();
			});

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var parameter = new Parameter();
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

			var child_parent = parent.AsContext<GroupChild.FakeContextChild>();
			Assert.IsInstanceOfType(child_parent, typeof(ICommand<GroupChild.FakeContextChild>));
			var child_child = child.AsContext<GroupChild.FakeContextChild>();
			Assert.IsInstanceOfType(child_child, typeof(ICommand<GroupChild.FakeContextChild>));

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var parent_child = child.AsContext<FakeContext>();
			});
			var parent_parent = parent.AsContext<FakeContext>();
			Assert.IsInstanceOfType(parent_parent, typeof(ICommand<FakeContext>));
		}

		[TestMethod]
		public void GetCommandById_Test()
		{
			var command = _Commands.GetCommandById<FakeContext>(NORM_ID);
			Assert.IsNotNull(command);

			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				_Commands.GetCommandById<FakeContext>("doesn't exist");
			});

			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				_Commands.GetCommandById<FakeContext>(DUPE_ID);
			});
		}

		[TestMethod]
		public void GetCommandsById_Test()
		{
			{
				var commands = _Commands.GetCommandsById<FakeContext>(DUPE_ID);
				Assert.AreEqual(2, commands.Count());
			}

			{
				var commands = _Commands.GetCommandsById<FakeContext>(NORM_ID);
				Assert.AreEqual(1, commands.Count());
			}

			{
				var commands = _Commands.GetCommandsById<FakeContext>("doesn't exist");
				Assert.AreEqual(0, commands.Count());
			}
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
				var parameters = _Commands.GetCommandsByType<NeverContext>();
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

		private class GroupChild : CommandGroup<GroupChild.FakeContextChild>
		{
			[Command("joe")]
			public void CommandChild()
			{
			}

			public class FakeContextChild : FakeContext
			{
			}
		}

		private class NeverContext : IContext
		{
			public Guid Id { get; set; }
			public IServiceProvider Services { get; set; } = EmptyServiceProvider.Instance;
		}

		private sealed class NotSevenPM : Precondition<FakeContext>
		{
			public override Task<IResult> CheckAsync(IImmutableCommand command, FakeContext context)
			{
				if (DateTime.UtcNow.Hour != 19)
				{
					return SuccessResult.Instance.Task;
				}
				return Result.FromError("It's seven PM.").AsTask();
			}
		}
	}
}
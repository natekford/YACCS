using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Tests.Commands.Models
{
	[TestClass]
	public class ReflectionCommand_Tests
	{
		[TestMethod]
		public async Task GroupInjection_Tests()
		{
			var method = typeof(GroupInjection)
				.GetMethod(nameof(GroupInjection.CommandAsync));
			var command = new ReflectionCommand(method!).ToImmutable().Single();
			var context = new FakeContext
			{
				Services = new ServiceCollection()
					.AddSingleton(GroupInjection.INJECTED_VALUE)
					.BuildServiceProvider(),
			};

			var result = await command.ExecuteAsync(context, Array.Empty<object?>()).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
		}

		[TestMethod]
		public void GroupMissingConstructor_Test()
		{
			var method = typeof(GroupMissingConstructor)
				.GetMethod(nameof(GroupMissingConstructor.CommandAsync));

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var command = new ReflectionCommand(method!);
			});
		}

		[TestMethod]
		public void GroupMissingInterface_Test()
		{
			var method = typeof(GroupMissingInterface)
				.GetMethod(nameof(GroupMissingInterface.CommandAsync));

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var command = new ReflectionCommand(method!);
			});
		}

		private class GroupBase : CommandGroup<FakeContext>
		{
			[Command("joeba")]
			public Task<IResult> CommandAsync() => SuccessResult.Instance.Task;
		}

		private class GroupInjection : GroupBase
		{
			public const string INJECTED_VALUE = "injected";

			public string? InjectMeField;
			public string? InjectMeProperty { get; set; }
			public string? WontBeInjectedBecauseNoSetter { get; }
			public Guid? WontBeInjectedBecauseNotRegistered { get; set; }

			public override Task AfterExecutionAsync(IImmutableCommand command, FakeContext context)
			{
				Assert.AreEqual(INJECTED_VALUE, InjectMeField);
				Assert.AreEqual(INJECTED_VALUE, InjectMeProperty);
				Assert.IsNull(WontBeInjectedBecauseNotRegistered);
				Assert.IsNull(WontBeInjectedBecauseNoSetter);

				return Task.CompletedTask;
			}
		}

		private class GroupMissingConstructor : GroupBase
		{
			public string Value { get; }

			public GroupMissingConstructor(string value)
			{
				Value = value;
			}
		}

		private class GroupMissingInterface
		{
			public string Value { get; }

			public GroupMissingInterface()
			{
				Value = "";
			}

			[Command("joeba")]
			public Task<IResult> CommandAsync() => SuccessResult.Instance.Task;
		}
	}
}
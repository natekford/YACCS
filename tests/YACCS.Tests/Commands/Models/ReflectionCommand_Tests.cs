using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Tests.Commands.Models;

[TestClass]
public class ReflectionCommand_Tests
{
	[TestMethod]
	public async Task GroupInjection_Test()
	{
		var method = typeof(GroupInjection)
			.GetMethod(nameof(GroupInjection.CommandAsync));
		var command = new ReflectionCommand(method!).ToImmutable();
		var context = new FakeContext
		{
			Services = new ServiceCollection()
				.AddSingleton(GroupInjection.INJECTED_VALUE)
				.BuildServiceProvider(),
		};

		var result = await command.ExecuteAsync(context, Array.Empty<object?>()).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
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
		public IResult CommandAsync() => Success.Instance;
	}

	private class GroupInjection : GroupBase
	{
		public const string INJECTED_VALUE = "injected";

		[InjectService]
		public string? InjectMeField;
		[InjectService]
		public string? InjectMeProperty { get; set; }
		public string? WontBeInjectedBecauseNoSetter { get; }
		public Guid? WontBeInjectedBecauseProtectedSetter { get; protected set; }

		public override Task AfterExecutionAsync(IImmutableCommand command, FakeContext context, IResult result)
		{
			Assert.AreEqual(INJECTED_VALUE, InjectMeField);
			Assert.AreEqual(INJECTED_VALUE, InjectMeProperty);
			Assert.IsNull(WontBeInjectedBecauseProtectedSetter);
			Assert.IsNull(WontBeInjectedBecauseNoSetter);

			return base.AfterExecutionAsync(command, context, result);
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
		public IResult CommandAsync() => Success.Instance;
	}
}
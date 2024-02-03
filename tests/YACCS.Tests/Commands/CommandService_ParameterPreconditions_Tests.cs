using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Localization;

namespace YACCS.Tests.Commands;

[TestClass]
public class CommandService_ParameterPreconditions_Tests
{
	[TestMethod]
	public async Task EnumerableFailure_Test()
	{
		const int DISALLOWED_VALUE = 1;

		var (context, command, parameter) = Create(DISALLOWED_VALUE);
		var arg = new[] { DISALLOWED_VALUE, DISALLOWED_VALUE + 1, DISALLOWED_VALUE + 2 };
		var result = await command.CanExecuteAsync(parameter, context, arg).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
		Assert.IsFalse(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	[TestMethod]
	public async Task EnumerableSuccess_Test()
	{
		const int DISALLOWED_VALUE = 1;

		var (context, command, parameter) = Create(DISALLOWED_VALUE);
		var arg = new[] { DISALLOWED_VALUE + 1, DISALLOWED_VALUE + 2, DISALLOWED_VALUE + 3 };
		var result = await command.CanExecuteAsync(parameter, context, arg).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsTrue(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	[TestMethod]
	public async Task NonEnumerableFailure_Test()
	{
		const int DISALLOWED_VALUE = 1;

		var (context, command, parameter) = Create(DISALLOWED_VALUE);
		var result = await command.CanExecuteAsync(parameter, context, DISALLOWED_VALUE).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
		Assert.IsFalse(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	[TestMethod]
	public async Task NonEnumerableSuccess_Test()
	{
		const int DISALLOWED_VALUE = 1;

		var (context, command, parameter) = Create(DISALLOWED_VALUE);
		var result = await command.CanExecuteAsync(parameter, context, 1 + DISALLOWED_VALUE).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsTrue(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	private static (FakeContext, IImmutableCommand, IImmutableParameter) Create(int disallowedValue)
	{
		static void Delegate(int arg)
		{
		}

		var commandBuilder = new DelegateCommand(Delegate, Array.Empty<LocalizedPath>());
		commandBuilder.Parameters[0]
			.AsType<int>()
			.AddParameterPrecondition(new FakeParameterPreconditionAttribute(disallowedValue))
			.AddParameterPrecondition(new WasIReachedParameterPreconditionAttribute());

		var command = commandBuilder.ToImmutable();
		var context = new FakeContext();
		return (context, command, command.Parameters[0]);
	}
}

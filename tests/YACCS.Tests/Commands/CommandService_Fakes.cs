using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Commands;

[Command(_Name)]
public class CommandsGroup : CommandGroup<FakeContext>
{
	public const string _1 = "c1_id";
	public const string _2 = "c2_id";
	public const string _3 = "c3_id";
	public const string _Delay = "delay";
	public const string _DelayedMessage = "delayed message";
	public const string _Disabled = "disabled";
	public const string _DisabledMessage = "lol disabled";
	public const string _Name = "joeba";
	public const string _Throws = "throws";
	public const string _ThrowsAfter = "throwsafter";
	public const string _ThrowsBefore = "throwsbefore";
	public const int DELAY = 250;

	[Command]
	[Id(_1)]
	public static void CommandOne()
	{
	}

	[Command]
	[Id(_2)]
	[Priority(1000)]
	public static void CommandTwo()
	{
	}

	[Command(_Delay)]
	public async Task<IResult> Delay()
	{
		await Task.Delay(DELAY).ConfigureAwait(false);
		return new Failure(_DelayedMessage);
	}

	[Command(_Disabled)]
	[DisabledPrecondition]
	[Priority(2)]
	public void Disabled()
	{
	}

	[Command]
	public void TestCommand()
	{
	}

	[Command]
	public void TestCommand(int input)
	{
	}

	[Command]
	public void TestCommand([Length(2)] IEnumerable<int> input)
	{
	}

	[Command]
	[Id(_3)]
	[Priority(1)]
	public void TestCommandWithRemainder([Remainder] int input)
	{
	}

	[Command(_Throws)]
	public void Throws()
		=> throw new InvalidOperationException();

	[Command(_ThrowsAfter)]
	[FakePreconditionWhichThrowsAfter]
	public void ThrowsAfter()
	{
	}

	[Command(_ThrowsBefore)]
	[FakePreconditionWhichThrowsBefore]
	public void ThrowsBefore()
	{
	}

	private class DisabledPrecondition : Precondition<FakeContext>
	{
		private static readonly IResult _Failure = new Failure(_DisabledMessage);

		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			FakeContext context)
			=> new(_Failure);
	}

	private class FakePreconditionWhichThrowsAfter : Precondition<FakeContext>
	{
		public override Task AfterExecutionAsync(
			IImmutableCommand command,
			FakeContext context,
			Exception? exception)
			=> throw new InvalidOperationException();

		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			FakeContext context)
			=> new(Success.Instance);
	}

	private class FakePreconditionWhichThrowsBefore : Precondition<FakeContext>
	{
		public override Task BeforeExecutionAsync(
			IImmutableCommand command,
			FakeContext context)
			=> throw new InvalidOperationException();

		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			FakeContext context)
			=> new(Success.Instance);
	}
}

public class FakeParameterPreconditionAttribute(int value)
	: ParameterPrecondition<FakeContext, int>
{
	public int DisallowedValue { get; } = value;

	public override ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		FakeContext context,
		[MaybeNull] int value)
		=> new(value == DisallowedValue ? new Failure("lol") : Success.Instance);
}

public class FakePrecondition(bool success) : Precondition<FakeContext>
{
	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		FakeContext context)
		=> new(success ? Success.Instance : Failure.Instance);
}

public class WasIReachedParameterPreconditionAttribute : ParameterPrecondition<FakeContext, int>
{
	public bool IWasReached { get; private set; }

	public override ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		FakeContext context,
		[MaybeNull] int value)
	{
		IWasReached = true;
		return new(Success.Instance);
	}
}

public class WasIReachedPrecondition : Precondition<FakeContext>
{
	public bool IWasReached { get; private set; }

	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		FakeContext context)
	{
		IWasReached = true;
		return new(default(IResult)!);
	}
}
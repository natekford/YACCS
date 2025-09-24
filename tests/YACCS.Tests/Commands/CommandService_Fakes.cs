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
		return Result.Failure(_DelayedMessage);
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

	private class DisabledPrecondition : SummarizablePrecondition<FakeContext>
	{
		private static readonly Result _Failure = Result.Failure(_DisabledMessage);

		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			FakeContext context)
			=> new(_Failure);

		public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
			=> throw new NotImplementedException();
	}

	private class FakePreconditionWhichThrowsAfter
		: SummarizablePrecondition<FakeContext>
	{
		public override Task AfterExecutionAsync(
			IImmutableCommand command,
			FakeContext context,
			Exception? exception)
			=> throw new InvalidOperationException();

		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			FakeContext context)
			=> new(Result.EmptySuccess);

		public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
			=> throw new NotImplementedException();
	}

	private class FakePreconditionWhichThrowsBefore
		: SummarizablePrecondition<FakeContext>
	{
		public override Task BeforeExecutionAsync(
			IImmutableCommand command,
			FakeContext context)
			=> throw new InvalidOperationException();

		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			FakeContext context)
			=> new(Result.EmptySuccess);

		public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
			=> throw new NotImplementedException();
	}
}

public class FakeParameterPreconditionAttribute(int value)
	: SummarizableParameterPrecondition<FakeContext, int>
{
	public int DisallowedValue { get; } = value;

	public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
		=> throw new NotImplementedException();

	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		FakeContext context,
		int value)
		=> new(value == DisallowedValue ? Result.Failure("lol") : Result.EmptySuccess);
}

public class FakePrecondition(bool success)
	: SummarizablePrecondition<FakeContext>
{
	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		FakeContext context)
		=> new(success ? Result.EmptySuccess : Result.EmptyFailure);

	public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
		=> throw new NotImplementedException();
}

public class WasIReachedParameterPreconditionAttribute
	: SummarizableParameterPrecondition<FakeContext, int>
{
	public bool IWasReached { get; private set; }

	public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
		=> throw new NotImplementedException();

	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		FakeContext context,
		int value)
	{
		IWasReached = true;
		return new(Result.EmptySuccess);
	}
}

public class WasIReachedPrecondition : SummarizablePrecondition<FakeContext>
{
	public bool IWasReached { get; private set; }

	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		FakeContext context)
	{
		IWasReached = true;
		return new(default(IResult)!);
	}

	public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
		=> throw new NotImplementedException();
}
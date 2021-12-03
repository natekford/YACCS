using System.Collections.Immutable;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.SwappedArguments;

/// <summary>
/// A generated command where arguments are in a different order.
/// </summary>
public class SwappedArgumentsCommand : GeneratedCommand
{
	private readonly Swapper _Swapper;

	/// <inheritdoc />
	public override IReadOnlyList<IImmutableParameter> Parameters { get; }
	/// <inheritdoc />
	public override int Priority { get; }

	/// <summary>
	/// Creates a new <see cref="SwappedArgumentsCommand"/>.
	/// </summary>
	/// <inheritdoc cref="GeneratedCommand(IImmutableCommand, int)"/>
	/// <param name="source"></param>
	/// <param name="priorityDifference"></param>
	/// <param name="swapper">The swapper to use for swapping arguments.</param>
	public SwappedArgumentsCommand(
		IImmutableCommand source,
		int priorityDifference,
		Swapper swapper) : base(source, priorityDifference)
	{
		_Swapper = swapper;

		var builder = ImmutableArray.CreateBuilder<IImmutableParameter>(Source.Parameters.Count);
		builder.AddRange(source.Parameters);
		_Swapper.Swap(builder);
		Parameters = builder.MoveToImmutable();
	}

	/// <inheritdoc />
	public override ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
	{
		var copy = args.ToArray();
		_Swapper.SwapBack(copy);
		return Source.ExecuteAsync(context, copy);
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;

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
		Parameters = [.. _Swapper.SwapForwards(source.Parameters)];
	}

	/// <inheritdoc />
	public override ValueTask<IResult> ExecuteAsync(
		IContext context,
		IReadOnlyList<object?> args)
		=> Source.ExecuteAsync(context, _Swapper.SwapBackwards(args));
}
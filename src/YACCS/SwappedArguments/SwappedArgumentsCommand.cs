using System.Collections.Immutable;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.SwappedArguments
{
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
		/// <param name="swapper">The swapper to use for swapping arguments.</param>
		/// <param name="priorityDifference">
		/// The amount to lower priority by for each step taken in reordering the indices.
		/// </param>
		/// <inheritdoc cref="GeneratedCommand(IImmutableCommand)"/>
		/// <param name="source"></param>
		public SwappedArgumentsCommand(
			IImmutableCommand source,
			Swapper swapper,
			int priorityDifference) : base(source)
		{
			_Swapper = swapper;
			Priority = source.Priority - (priorityDifference * _Swapper.Swaps.Length);

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
}
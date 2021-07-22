using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.SwappedArguments
{
	public class SwappedArgumentsCommand : GeneratedCommand
	{
		private readonly Swapper _Swapper;

		public override IReadOnlyList<IImmutableParameter> Parameters { get; }
		public override int Priority { get; }

		public SwappedArgumentsCommand(
			IImmutableCommand source,
			Swapper swapper,
			int priorityDifference) : base(source)
		{
			_Swapper = swapper;
			Priority = source.Priority + (priorityDifference * _Swapper.Swaps.Length);

			var builder = ImmutableArray.CreateBuilder<IImmutableParameter>(Source.Parameters.Count);
			builder.AddRange(source.Parameters);
			_Swapper.Swap(builder);
			Parameters = builder.MoveToImmutable();
		}

		public override ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
		{
			var copy = args.ToArray();
			_Swapper.SwapBack(copy);
			return Source.ExecuteAsync(context, copy);
		}
	}
}
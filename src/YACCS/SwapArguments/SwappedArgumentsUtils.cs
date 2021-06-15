using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.SwapArguments
{
	public static class SwappedArgumentsUtils
	{
		public static IEnumerable<SwappedArgumentsCommand> GenerateSwappedArgumentsVersions(
			this IImmutableCommand command,
			int priorityDifference)
		{
			var indices = new List<int>(command.Parameters.Count);
			for (var i = 0; i < command.Parameters.Count; ++i)
			{
				if (command.Parameters[i].Get<ISwappableAttribute>().Any())
				{
					indices.Add(i);
				}
			}
			return command.GenerateSwappedArgumentsVersions(indices, priorityDifference);
		}

		public static IEnumerable<SwappedArgumentsCommand> GenerateSwappedArgumentsVersions(
			this IImmutableCommand command,
			IReadOnlyList<int> indices,
			int priorityDifference)
		{
			foreach (var swapper in Swapper.CreateSwappers(indices))
			{
				yield return new(command, swapper, priorityDifference);
			}
		}
	}
}
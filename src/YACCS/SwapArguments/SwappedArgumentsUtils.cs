using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.SwapArguments
{
	public static class SwappedArgumentsUtils
	{
		public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> result = new[] { Enumerable.Empty<T>() };
			foreach (var sequence in sequences)
			{
				result = result.SelectMany(_ => sequence, (seq, x) => seq.Append(x));
			}
			return result;
		}

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
			var totalPermutations = Factorial(indices.Count);
			for (var i = 0; i < indices.Count; ++i)
			{
				var builder = ImmutableArray.CreateBuilder<int>(indices.Count);
#warning does nothing right now
				yield return new(command, builder.MoveToImmutable(), priorityDifference);
			}
		}

		private static int Factorial(int i)
			=> i <= 1 ? 1 : i * Factorial(i - 1);
	}
}
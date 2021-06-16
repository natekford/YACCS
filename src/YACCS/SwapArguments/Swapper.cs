using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace YACCS.SwapArguments
{
	public sealed class Swapper
	{
		public ImmutableArray<int> Indices { get; }
		public ImmutableArray<(int, int)> Swaps { get; }

		public Swapper(IEnumerable<int> indices)
		{
			var copy = indices.ToArray();
			Indices = Unsafe.As<int[], ImmutableArray<int>>(ref copy);

			var swaps = new List<(int, int)>();
			for (var i = 0; i < copy.Length - 1; ++i)
			{
				var minIndex = i;
				for (var j = i + 1; j < copy.Length; ++j)
				{
					if (copy[j] < copy[minIndex])
					{
						minIndex = j;
					}
				}

				if (copy[minIndex] != copy[i])
				{
					swaps.Add((copy[minIndex], copy[i]));
					Swap(copy, minIndex, i);
				}
			}
			Swaps = swaps.ToImmutableArray();
		}

		public static IEnumerable<Swapper> CreateSwappers(IEnumerable<int> indices)
		{
			static IEnumerable<IEnumerable<T>> GetPermutations<T>(
				IEnumerable<T> sequence,
				int length)
			{
				if (length == 1)
				{
					return sequence.Select(x => new[] { x });
				}

				IEnumerable<T> CollectionSelector(IEnumerable<T> x)
					=> sequence.Where(e => !x.Contains(e));

				IEnumerable<T> ResultSelector(IEnumerable<T> seq, T x)
					=> seq.Append(x);

				return GetPermutations(sequence, length - 1)
					.SelectMany(CollectionSelector, ResultSelector);
			}

			var ordered = indices.OrderBy(x => x).ToArray();
			IEnumerable<int> RemoveOrderedIndices(IEnumerable<int> source)
			{
				var i = 0;
				foreach (var index in source)
				{
					if (ordered[i] != index)
					{
						yield return index;
					}
					++i;
				}
			}

			var permutations = GetPermutations(indices, ordered.Length);
			return permutations
				.Select(RemoveOrderedIndices)
				.Where(x => x.Any())
				.Select(x => new Swapper(x));
		}

		public void Swap<T>(IList<T> source)
		{
			for (var i = 0; i < Swaps.Length; ++i)
			{
				Swap(source, Swaps[i]);
			}
		}

		public void SwapBack<T>(IList<T> source)
		{
			for (var i = Swaps.Length - 1; i >= 0; --i)
			{
				Swap(source, Swaps[i]);
			}
		}

		private static void Swap<T>(IList<T> source, (int Left, int Right) indices)
			=> Swap(source, indices.Left, indices.Right);

		private static void Swap<T>(IList<T> source, int left, int right)
		{
			var temp = source[left];
			source[left] = source[right];
			source[right] = temp;
		}
	}
}
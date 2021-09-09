using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace YACCS.SwappedArguments
{
	/// <summary>
	/// Swaps items to a new order and swaps them back to their original order.
	/// </summary>
	public sealed class Swapper
	{
		/// <summary>
		/// The indices that will get swapped.
		/// </summary>
		public ImmutableArray<int> Indices { get; }
		/// <summary>
		/// The steps to take to swap <see cref="Indices"/>.
		/// </summary>
		public ImmutableArray<(int, int)> Swaps { get; }

		/// <summary>
		/// Creates a new <see cref="Swapper"/>.
		/// </summary>
		/// <param name="indices">
		/// <inheritdoc cref="Indices" path="/summary"/>
		/// </param>
		public Swapper(IEnumerable<int> indices)
		{
			var copy = indices.ToList();
			Indices = copy.ToImmutableArray();

			var swaps = new List<(int, int)>();
			for (var i = 0; i < copy.Count - 1; ++i)
			{
				var minIndex = i;
				for (var j = i + 1; j < copy.Count; ++j)
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

		/// <summary>
		/// Creates permutations of <paramref name="indices"/> and then removes any indices
		/// which are in order.
		/// </summary>
		/// <param name="indices">The indices to create permutations for.</param>
		/// <returns>
		/// An enumerable of every permutation which does not become empty after all
		/// indices that are in order are removed.
		/// </returns>
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

		/// <summary>
		/// Swaps the items which have indices in <see cref="Indices"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The list to reorder.</param>
		public void Swap<T>(IList<T> source)
		{
			for (var i = 0; i < Swaps.Length; ++i)
			{
				Swap(source, Swaps[i]);
			}
		}

		/// <summary>
		/// Returns the items to their original index.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The list to reorder.</param>
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
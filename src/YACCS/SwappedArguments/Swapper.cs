using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace YACCS.SwappedArguments;

/// <summary>
/// Swaps items to a new order and swaps them back to their original order.
/// </summary>
public sealed class Swapper
{
	/// <summary>
	/// The indices to move when going from an unswapped list to swapped list.
	/// </summary>
	public FrozenDictionary<int, int> MapBackward { get; }
	/// <summary>
	/// The indices to move when going from a swapped list to unswapped list.
	/// </summary>
	public FrozenDictionary<int, int> MapForward { get; }

	/// <summary>
	/// Creates a new <see cref="Swapper"/>.
	/// </summary>
	/// <param name="indices">The indices to swap.</param>
	public Swapper(IEnumerable<int> indices)
	{
		MapForward = indices
			.OrderBy(x => x)
			.Zip(indices, (a, b) => (A: a, B: b))
			.ToFrozenDictionary(x => x.A, x => x.B);

		MapBackward = MapForward.ToFrozenDictionary(x => x.Value, x => x.Key);
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
	/// Creates a new list with the items in the original order.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="source">The list to reorder.</param>
	public IReadOnlyList<T> SwapBackwards<T>(IReadOnlyList<T> source)
		=> new SwapWrapper<T>(source, MapBackward);

	/// <summary>
	/// Creates a new list with the items in the desired order.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="source">The list to reorder.</param>
	public IReadOnlyList<T> SwapForwards<T>(IReadOnlyList<T> source)
		=> new SwapWrapper<T>(source, MapForward);

	private sealed class SwapWrapper<T>(
		IReadOnlyList<T> source,
		FrozenDictionary<int, int> map)
		: IReadOnlyList<T>
	{
		public int Count => source.Count;

		public T this[int index]
			=> map.TryGetValue(index, out var i) ? source[i] : source[index];

		public IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < Count; ++i)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace YACCS.SwapArguments
{
	public sealed class Swapper
	{
		private readonly ImmutableArray<int> _OrderedIndices;

		public ImmutableArray<int> Indices { get; }

		public Swapper(IEnumerable<int> indices)
		{
			Indices = indices.ToImmutableArray();
			_OrderedIndices = Indices.OrderBy(x => x).ToImmutableArray();
		}

		public static IEnumerable<Swapper> CreateSwappers(IReadOnlyList<int> indices)
		{
			IEnumerable<int> RemoveOrderedIndices(IEnumerable<int> source)
			{
				var i = 0;
				foreach (var index in source)
				{
					if (indices[i] != index)
					{
						yield return index;
					}
					++i;
				}
			}

			var permutations = GetPermutations(indices, indices.Count);
			return permutations
				.Select(RemoveOrderedIndices)
				.Where(x => x.Any())
				.Select(x => new Swapper(x));
		}

		public void Swap<T>(IList<T> source)
			=> Move(Indices, _OrderedIndices, source);

		public void SwapBack<T>(IList<T> source)
			=> Move(_OrderedIndices, Indices, source);

		private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> sequence, int length)
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

		private static void Move<T>(ImmutableArray<int> a, ImmutableArray<int> b, IList<T> source)
		{
			var temp = new T[a.Length];
			for (var i = 0; i < a.Length; ++i)
			{
				temp[i] = source[a[i]];
			}
			for (var i = 0; i < b.Length; ++i)
			{
				source[b[i]] = temp[i];
			}
		}
	}
}
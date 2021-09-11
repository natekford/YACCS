using System;
using System.Collections.Generic;

namespace YACCS.Trie
{
	/// <summary>
	/// Utilities for tries.
	/// </summary>
	public static class TrieUtils
	{
		/// <summary>
		/// Returns all distinct items directly inside <paramref name="node"/> and recursively
		/// inside all of its edges until every edge that has <paramref name="node"/> as an
		/// ancestor has been iterated through.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="node">The node to get items from.</param>
		/// <param name="predicate">The predicate to check nodes with.</param>
		/// <returns>
		/// A set of all distinct nodes from <paramref name="node"/> and its edges.
		/// </returns>
		public static HashSet<TValue> GetAllDistinctItems<TKey, TValue>(
			this INode<TKey, TValue> node,
			Func<TValue, bool>? predicate = null)
		{
			static IEnumerable<TValue> GetItems(INode<TKey, TValue> node)
			{
				foreach (var item in node.Items)
				{
					yield return item;
				}
				foreach (var edge in node.Edges)
				{
					foreach (var item in GetItems(edge))
					{
						yield return item;
					}
				}
			}

			predicate ??= _ => true;

			var set = new HashSet<TValue>();
			foreach (var item in GetItems(node))
			{
				if (predicate(item))
				{
					set.Add(item);
				}
			}
			return set;
		}
	}
}
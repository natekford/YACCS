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
		/// Follows <paramref name="path"/> down the edges of a node.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="node">The node to start from.</param>
		/// <param name="path">The path to go down.</param>
		/// <returns>
		/// A node if one is at the end of <paramref name="path"/>,
		/// otherwise <see langword="null"/>.
		/// </returns>
		public static INode<TKey, TValue>? FollowPath<TKey, TValue>(
			this INode<TKey, TValue> node,
			ReadOnlySpan<TKey> path)
		{
			foreach (var key in path)
			{
				if (!node.TryGetEdge(key, out node!))
				{
					break;
				}
			}
			return node;
		}

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
				foreach (var item in node)
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
using System;
using System.Collections.Generic;

namespace YACCS.Trie;

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

	/// <inheritdoc cref="FollowPath{TKey, TValue}(INode{TKey, TValue}, ReadOnlySpan{TKey})" />
	public static INode<TKey, TValue>? FollowPath<TKey, TValue>(
		this INode<TKey, TValue> node,
		IEnumerable<TKey> path)
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
	/// Returns all items directly inside <paramref name="node"/> and recursively
	/// inside all of its edges until every edge that has <paramref name="node"/> as an
	/// ancestor has been iterated through.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="node">The node to get items from.</param>
	/// <param name="recursive">Whether to look deeper than the supplied node.</param>
	/// <returns>
	/// An enumerable of all nodes from <paramref name="node"/> and its edges.
	/// </returns>
	public static IEnumerable<TValue> GetItems<TKey, TValue>(
		this INode<TKey, TValue> node,
		bool recursive)
	{
		foreach (var item in node)
		{
			yield return item;
		}
		if (recursive)
		{
			foreach (var edge in node.Edges)
			{
				foreach (var item in GetItems(edge, recursive))
				{
					yield return item;
				}
			}
		}
	}

	/// <summary>
	/// Returns all items at the end of each <paramref name="paths"/> after starting at
	/// <paramref name="node"/>.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="node">The node to start searching for subitems in.</param>
	/// <param name="paths">The paths to follow starting at <paramref name="node"/>.</param>
	/// <returns>An enumerable of items at the end of each path.</returns>
	public static IEnumerable<TValue> GetSubitems<TKey, TValue>(
		this INode<TKey, TValue> node,
		IEnumerable<IEnumerable<TKey>> paths)
	{
		foreach (var path in paths)
		{
			var followed = node.FollowPath(path);
			if (followed is null)
			{
				continue;
			}

			foreach (var edge in followed.Edges)
			{
				foreach (var subitem in edge)
				{
					yield return subitem;
				}
			}
		}
	}
}
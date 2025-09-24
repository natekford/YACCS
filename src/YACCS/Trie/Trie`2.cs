﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;

namespace YACCS.Trie;

/// <inheritdoc cref="ITrie{TKey, TValue}" />
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public abstract class Trie<TKey, TValue> : ITrie<TKey, TValue>
{
	private readonly HashSet<TValue> _Items = [];
	private readonly Node _Root;

	/// <inheritdoc />
	public int Count => _Items.Count;
	/// <inheritdoc />
	public bool IsReadOnly => false;
	/// <inheritdoc />
	public INode<TKey, TValue> Root => _Root;
	private string DebuggerDisplay => $"Count = {Count}";

	/// <summary>
	/// Creates a new <see cref="Trie{TKey, TValue}"/>.
	/// </summary>
	/// <param name="comparer">The comparer to use when comparing keys.</param>
	protected Trie(IEqualityComparer<TKey> comparer)
	{
		_Root = new(default!, null, comparer);
	}

	/// <inheritdoc />
	public virtual void Add(TValue item)
	{
		if (!_Items.Add(item))
		{
			return;
		}

		foreach (var path in GetPaths(item))
		{
			var node = _Root;
			foreach (var key in path)
			{
				node = node.GetOrAdd(key);
			}
			// Node will always not be null because we add any missing paths
			node.Add(item);
		}
	}

	/// <inheritdoc />
	public void Clear()
	{
		_Root.Clear();
		_Items.Clear();
	}

	/// <inheritdoc />
	public bool Contains(TValue item)
		=> _Items.Contains(item);

	/// <inheritdoc />
	public void CopyTo(TValue[] array, int arrayIndex)
		=> _Items.CopyTo(array, arrayIndex);

	/// <inheritdoc />
	public IEnumerator<TValue> GetEnumerator()
		=> _Items.GetEnumerator();

	/// <inheritdoc />
	public bool Remove(TValue item)
	{
		if (!_Items.Remove(item))
		{
			return false;
		}

		var removed = false;
		foreach (var path in GetPaths(item))
		{
			var node = _Root;
			foreach (var segment in path)
			{
				// If a segment of the path cannot be found then this path is
				// not in the trie
				if (!node.TryGetEdge(segment, out node))
				{
					break;
				}
			}
			// Node will only not be null if the path is successful
			if (node?.Remove(item) == true)
			{
				removed = true;
			}
		}
		return removed;
	}

	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

	/// <summary>
	/// Gets the paths to use as keys for <paramref name="item"/>.
	/// </summary>
	/// <param name="item">The item to get paths for.</param>
	/// <returns>An enumerable of paths.</returns>
	protected abstract IEnumerable<IReadOnlyList<TKey>> GetPaths(TValue item);

	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	private class Node(TKey key, Node? parent, IEqualityComparer<TKey> comparer)
		: INode<TKey, TValue>
	{
		private readonly IEqualityComparer<TKey> _Comparer = comparer;
		private readonly Dictionary<TKey, Node> _Edges = new(comparer);
		private readonly HashSet<TValue> _Items = [];
		private readonly TKey _Key = key;
		private readonly Node? _Parent = parent;

		public int Count => _Items.Count;
		public IReadOnlyCollection<Node> Edges => _Edges.Values;
		IReadOnlyCollection<INode<TKey, TValue>> INode<TKey, TValue>.Edges => Edges;
		private string DebuggerDisplay
		{
			get
			{
				var path = string.Empty;
				for (var node = this; node._Parent is not null; node = node._Parent)
				{
					path = $"{node._Key} {path}";
				}
				if (string.IsNullOrWhiteSpace(path))
				{
					path = "ROOT";
				}
				return $"Path = {path.TrimEnd()}, Count = {_Items.Count}";
			}
		}
		public Node this[TKey key]
			=> _Edges[key];

		INode<TKey, TValue> INode<TKey, TValue>.this[TKey key]
			=> this[key];

		public bool Add(TValue item)
			=> _Items.Add(item);

		public void Clear()
		{
			_Edges.Clear();
			_Items.Clear();
		}

		public IEnumerator<TValue> GetEnumerator()
			=> _Items.GetEnumerator();

		public Node GetOrAdd(TKey key)
		{
			if (!_Edges.TryGetValue(key, out var node))
			{
				_Edges[key] = node = new(key, this, _Comparer);
			}
			return node;
		}

		public bool Remove(TValue item)
		{
			var isRemoved = _Items.Remove(item);
			if (isRemoved)
			{
				// Kill all empty nodes
				for (var node = this; node._Parent is not null; node = node._Parent)
				{
					if (node._Items.Count == 0 && node._Edges.Count == 0)
					{
						node._Parent._Edges.Remove(node._Key);
					}
				}
			}
			return isRemoved;
		}

		public bool TryGetEdge(TKey key, [NotNullWhen(true)] out Node? node)
			=> _Edges.TryGetValue(key, out node);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		bool INode<TKey, TValue>.TryGetEdge(
			TKey key,
			[NotNullWhen(true)] out INode<TKey, TValue>? node)
		{
			var result = TryGetEdge(key, out var temp);
			node = temp;
			return result;
		}
	}
}
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Commands
{
	/// <inheritdoc cref="ITrie{TKey, TValue}" />
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public abstract class Trie<TKey, TValue> : ITrie<TKey, TValue>
	{
		private readonly ConcurrentDictionary<TValue, byte> _Items;
		private readonly Node _Root;

		/// <inheritdoc />
		public bool IsReadOnly => false;
		/// <inheritdoc />
		public IReadOnlyCollection<TValue> Items => (IReadOnlyCollection<TValue>)_Items.Keys;
		/// <inheritdoc />
		public INode<TKey, TValue> Root => _Root;
		/// <inheritdoc />
		public int Count => _Items.Count;
		private string DebuggerDisplay => $"Count = {Count}";

		protected Trie(IEqualityComparer<TKey> comparer)
		{
			_Items = new();
			_Root = new(default!, null, comparer);
		}

		/// <inheritdoc />
		public virtual void Add(TValue item)
		{
			if (!_Items.TryAdd(item, 0))
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
			=> _Items.ContainsKey(item);

		/// <inheritdoc />
		public void CopyTo(TValue[] array, int arrayIndex)
		{
			foreach (var command in this)
			{
				array[arrayIndex++] = command;
			}
		}

		/// <inheritdoc />
		public IEnumerator<TValue> GetEnumerator()
			=> Items.GetEnumerator();

		/// <inheritdoc />
		public bool Remove(TValue item)
		{
			if (!_Items.TryRemove(item, out _))
			{
				return false;
			}

			var removed = false;
			foreach (var path in GetPaths(item))
			{
				var node = _Root;
				foreach (var key in path)
				{
					if (!node.TryGetEdge(key, out node))
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

		protected abstract IEnumerable<IReadOnlyList<TKey>> GetPaths(TValue item);

		[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
		private class Node : INode<TKey, TValue>
		{
			private readonly IEqualityComparer<TKey> _Comparer;
			private readonly ConcurrentDictionary<TKey, Node> _Edges;
			private readonly ConcurrentDictionary<TValue, byte> _Items;
			private readonly TKey _Key;
			private readonly Node? _Parent;

			public IReadOnlyCollection<TValue> Items
				=> (IReadOnlyCollection<TValue>)_Items.Keys;
			public IReadOnlyCollection<Node> Edges
				=> (IReadOnlyCollection<Node>)_Edges.Values;
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
					return $"Path = {path}, Count = {_Items.Count}";
				}
			}

			public Node this[TKey key]
			{
				get => _Edges[key];
				set => _Edges[key] = value;
			}
			INode<TKey, TValue> INode<TKey, TValue>.this[TKey key]
				=> this[key];

			public Node(TKey key, Node? parent, IEqualityComparer<TKey> comparer)
			{
				_Comparer = comparer;
				_Edges = new(comparer);
				_Items = new();
				_Key = key;
				_Parent = parent;
			}

			public bool Add(TValue item)
				=> _Items.TryAdd(item, 0);

			public void Clear()
			{
				_Edges.Clear();
				_Items.Clear();
			}

			public bool Contains(TValue item)
				// Only direct items since the node has already been found via name
				=> _Items.ContainsKey(item);

			public Node GetOrAdd(TKey key)
			{
				return _Edges.GetOrAdd(key, static (key, parent) =>
				{
					return new(key, parent, parent._Comparer);
				}, this);
			}

			public bool Remove(TValue item)
			{
				var isRemoved = _Items.TryRemove(item, out _);
				if (isRemoved)
				{
					// Kill all empty nodes
					for (var node = this; node._Parent is not null; node = node._Parent)
					{
						if (node._Items.Count == 0 && node._Edges.Count == 0)
						{
							node._Parent._Edges.TryRemove(node._Key, out _);
						}
					}
				}
				return isRemoved;
			}

			public bool TryGetEdge(TKey key, [NotNullWhen(true)] out Node? node)
				=> _Edges.TryGetValue(key, out node);

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
}
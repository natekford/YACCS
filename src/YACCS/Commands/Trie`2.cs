using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Commands
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public abstract class Trie<TKey, TValue> : ITrie<TKey, TValue>
	{
		private readonly IEqualityComparer<TKey> _Comparer;
		private readonly ConcurrentDictionary<TValue, byte> _Items;
		private readonly Node _Root;

		public bool IsReadOnly => false;
		public IReadOnlyCollection<TValue> Items => (IReadOnlyCollection<TValue>)_Items.Keys;
		public INode<TKey, TValue> Root => _Root;
		public int Count => _Items.Count;
		private string DebuggerDisplay => $"Count = {Count}";

		protected Trie(IEqualityComparer<TKey> comparer)
		{
			_Comparer = comparer;
			_Items = new();
			_Root = new(default!, null, comparer);
		}

		public virtual int Add(TValue item)
		{
			var added = 0;
			if (_Items.TryAdd(item, 0))
			{
				foreach (var paths in GetPaths(item))
				{
					var node = _Root;
					for (var i = 0; i < paths.Count; ++i)
					{
						var key = paths[i];
						if (!node.TryGetEdge(key, out var next))
						{
							node[key] = next = new(key, node, _Comparer);
						}
						if (i == paths.Count - 1 && next.Add(item))
						{
							++added;
						}
						node = next;
					}
				}
			}
			return added;
		}

		public void Clear()
		{
			_Root.Clear();
			_Items.Clear();
		}

		public bool Contains(TValue item)
			=> _Items.ContainsKey(item);

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			foreach (var command in this)
			{
				array[arrayIndex++] = command;
			}
		}

		public IEnumerator<TValue> GetEnumerator()
			=> Items.GetEnumerator();

		public int Remove(TValue item)
		{
			var removed = 0;
			if (_Items.TryRemove(item, out _))
			{
				foreach (var path in GetPaths(item))
				{
					var node = _Root;
					for (var i = 0; i < path.Count; ++i)
					{
						if (!node.TryGetEdge(path[i], out node))
						{
							break;
						}
						if (i == path.Count - 1 && node.Remove(item))
						{
							++removed;
						}
					}
				}
			}
			return removed;
		}

		void ICollection<TValue>.Add(TValue item)
			=> Add(item);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		bool ICollection<TValue>.Remove(TValue item)
			=> Remove(item) > 0;

		protected abstract IEnumerable<IReadOnlyList<TKey>> GetPaths(TValue item);

		[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
		protected class Node : INode<TKey, TValue>
		{
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

			public Node(TKey key, Node? parent, IEqualityComparer<TKey> stringComparer)
			{
				_Edges = new(stringComparer);
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
				node = temp!;
				return result;
			}
		}
	}
}
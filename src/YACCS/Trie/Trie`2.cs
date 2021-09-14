using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;

namespace YACCS.Trie
{
	/// <inheritdoc cref="IReadOnlyTrie{TKey, TValue}" />
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public abstract class Trie<TKey, TValue> : ITrie<TKey, TValue>
	{
		private readonly HashSet<TValue> _Items;
		private readonly Node _Root;

		/// <inheritdoc />
		public bool IsReadOnly => false;
		/// <inheritdoc />
		public INode<TKey, TValue> Root => _Root;
		/// <inheritdoc />
		public int Count => _Items.Count;
		private string DebuggerDisplay => $"Count = {Count}";

		/// <summary>
		/// Creates a new <see cref="Trie{TKey, TValue}"/>.
		/// </summary>
		/// <param name="comparer">The comparer to use when comparing keys.</param>
		protected Trie(IEqualityComparer<TKey> comparer)
		{
			_Items = new();
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

		/// <summary>
		/// Gets the paths to use as keys for <paramref name="item"/>.
		/// </summary>
		/// <param name="item">The item to get paths for.</param>
		/// <returns>An enumerable of paths.</returns>
		protected abstract IEnumerable<IReadOnlyList<TKey>> GetPaths(TValue item);

		[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
		private class Node : INode<TKey, TValue>
		{
			private readonly IEqualityComparer<TKey> _Comparer;
			private readonly Dictionary<TKey, Node> _Edges;
			private readonly HashSet<TValue> _Items;
			private readonly TKey _Key;
			private readonly Node? _Parent;

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
				=> _Items.Add(item);

			public void Clear()
			{
				_Edges.Clear();
				_Items.Clear();
			}

			public bool Contains(TValue item)
				// Only direct items since the node has already been found via name
				=> _Items.Contains(item);

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
}
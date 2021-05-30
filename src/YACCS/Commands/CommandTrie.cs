using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class CommandTrie : ITrie<IImmutableCommand>
	{
		private readonly ICommandServiceConfig _Config;
		private readonly IReadOnlyDictionary<Type, ITypeReader> _Readers;
		private HashSet<IImmutableCommand> _Items;
		private Node _Root;

		public bool IsReadOnly => false;
		public INode<IImmutableCommand> Root => _Root;
		public int Count => _Items.Count;
		private string DebuggerDisplay => $"Count = {Count}";

		public CommandTrie(
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ICommandServiceConfig config)
		{
			_Readers = readers;
			_Config = config;
			_Root = new Node(null, null, _Config.CommandNameComparer);
			_Items = new HashSet<IImmutableCommand>();
		}

		public int Add(IImmutableCommand item)
		{
			if (item.Names.Count == 0)
			{
				throw new ArgumentException("Cannot add a command with no name.", nameof(item));
			}

			// Very that every name is valid
			foreach (var name in item.Names)
			{
				foreach (var part in name)
				{
					if (part.Contains(_Config.Separator))
					{
						throw new ArgumentException($"Command names cannot contain the separator ({name}).", nameof(item));
					}
				}
			}

			// Verify that every parameter has a type reader
			foreach (var parameter in item.Parameters)
			{
				try
				{
					_ = _Readers.GetTypeReader(parameter);
				}
				catch (Exception ex)
				{
					throw new ArgumentException($"{parameter.ParameterName} does not have a registered type reader for {parameter.ParameterType}.", ex);
				}
			}

			var added = 0;
			foreach (var name in item.Names)
			{
				var node = _Root;
				for (var i = 0; i < name.Count; ++i)
				{
					var key = name[i];
					if (!node.TryGetEdge(key, out var next))
					{
						node[key] = next = new Node(key, node, _Config.CommandNameComparer);
					}
					if (i == name.Count - 1 && next.Add(item))
					{
						++added;
					}
					node = next;
				}
			}
			if (added != 0)
			{
				_Items.Add(item);
			}
			return added;
		}

		public void Clear()
		{
			_Root = new Node(null, null, _Config.CommandNameComparer);
			_Items = new HashSet<IImmutableCommand>();
		}

		public bool Contains(IImmutableCommand item)
		{
			// Since commands with no names can't get added, they will never be in the trie
			if (item.Names.Count == 0)
			{
				return false;
			}
			if (item.Names.Count >= _Items.Count)
			{
				return _Items.Contains(item);
			}

			foreach (var name in item.Names)
			{
				var node = _Root;
				for (var i = 0; i < name.Count; ++i)
				{
					if (!node.TryGetEdge(name[i], out node))
					{
						break;
					}
					if (i == name.Count - 1 && node.Contains(item))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void CopyTo(IImmutableCommand[] array, int arrayIndex)
		{
			foreach (var command in this)
			{
				array[arrayIndex++] = command;
			}
		}

		public IEnumerator<IImmutableCommand> GetEnumerator()
			=> _Items.GetEnumerator();

		public int Remove(IImmutableCommand item)
		{
			// Since commands with no names can't get added, they will never be in the trie
			// If the item isn't in the hashset containing all items, it's not in the trie
			if (item.Names.Count == 0 || !_Items.Remove(item))
			{
				return 0;
			}

			var removed = 0;
			foreach (var name in item.Names)
			{
				var node = _Root;
				for (var i = 0; i < name.Count; ++i)
				{
					if (!node.TryGetEdge(name[i], out node))
					{
						break;
					}
					if (i == name.Count - 1 && node.Remove(item))
					{
						++removed;
					}
				}
			}
			return removed;
		}

		void ICollection<IImmutableCommand>.Add(IImmutableCommand item)
			=> Add(item);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		bool ICollection<IImmutableCommand>.Remove(IImmutableCommand item)
			=> Remove(item) > 0;

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		private sealed class Node : INode<IImmutableCommand>
		{
			private readonly Dictionary<string, Node> _Edges;
			private readonly HashSet<IImmutableCommand> _Items;
			private readonly string? _Key;
			private readonly Node? _Parent;

			public IReadOnlyCollection<IImmutableCommand> Items => _Items;
			public IReadOnlyCollection<Node> Edges => _Edges.Values;
			IReadOnlyCollection<INode<IImmutableCommand>> INode<IImmutableCommand>.Edges => Edges;
			private string DebuggerDisplay
			{
				get
				{
					var path = "";
					for (var node = this; node is not null; node = node._Parent)
					{
						path = node._Key + " " + path;
					}
					if (string.IsNullOrWhiteSpace(path))
					{
						path = "ROOT";
					}
					return $"Path = {path}, Count = {_Items.Count}";
				}
			}

			public Node this[string key]
			{
				get => _Edges[key];
				set => _Edges[key] = value;
			}
			INode<IImmutableCommand> INode<IImmutableCommand>.this[string key]
				=> this[key];

			public Node(string? key, Node? parent, IEqualityComparer<string> stringComparer)
			{
				_Items = new HashSet<IImmutableCommand>();
				_Edges = new Dictionary<string, Node>(stringComparer);
				_Key = key;
				_Parent = parent;
			}

			public bool Add(IImmutableCommand command)
				=> _Items.Add(command);

			public bool Contains(IImmutableCommand command)
				// Only direct commands since the node has already been found via name
				=> _Items.Contains(command);

			public bool Remove(IImmutableCommand command)
			{
				if (!_Items.Remove(command))
				{
					return false;
				}

				// Kill all empty nodes
				for (var node = this; node is not null; node = node._Parent)
				{
					if (node._Items.Count == 0 && node._Edges.Count == 0)
					{
						node._Parent?._Edges?.Remove(node._Key!);
					}
				}

				return true;
			}

			public bool TryGetEdge(string key, [NotNullWhen(true)] out Node? node)
				=> _Edges.TryGetValue(key, out node);

			bool INode<IImmutableCommand>.TryGetEdge(string key, [NotNullWhen(true)] out INode<IImmutableCommand>? node)
			{
				var result = TryGetEdge(key, out var temp);
				node = temp!;
				return result;
			}
		}
	}
}
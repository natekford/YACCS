using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class CommandTrie : ITrie<IImmutableCommand>
	{
		private readonly ITypeRegistry<ITypeReader> _Readers;
		private readonly IEqualityComparer<string> _StringComparer;
		private Node _Root;

		public bool IsReadOnly => false;
		public INode<IImmutableCommand> Root => _Root;
		public int Count => _Root.AllValues.Count;
		private string DebuggerDisplay => $"Count = {Count}";

		public CommandTrie(IEqualityComparer<string> stringComparer, ITypeRegistry<ITypeReader> readers)
		{
			_StringComparer = stringComparer;
			_Readers = readers;
			_Root = new Node(null, null, _StringComparer);
		}

		public int Add(IImmutableCommand item)
		{
			foreach (var parameter in item.Parameters)
			{
				if (parameter.TypeReader == null
					&& !_Readers.TryGet(parameter.ParameterType, out _)
					&& (parameter.ElementType == null || !_Readers.TryGet(parameter.ElementType, out _)))
				{
					var param = parameter.ParameterType.Name;
					var cmd = item.Names?.FirstOrDefault()?.ToString() ?? "NO NAME";
					throw new ArgumentException($"A type reader for {param} in {cmd} is missing.", nameof(item));
				}
			}

			var added = 0;
			foreach (var name in item.Names)
			{
				var node = _Root;
				for (var i = 0; i < name.Parts.Count; ++i)
				{
					var key = name.Parts[i];
					if (!node.TryGetEdge(key, out var next))
					{
						node[key] = next = new Node(key, node, _StringComparer);
					}
					if (i == name.Parts.Count - 1 && next.Add(item))
					{
						++added;
					}
					node = next;
				}
			}
			return added;
		}

		public void Clear()
			=> _Root = new Node(null, null, _StringComparer);

		public bool Contains(IImmutableCommand item)
		{
			// Since commands with no names can't get added, they will never be in the trie
			if (item.Names.Count == 0)
			{
				return false;
			}
			if (item.Names.Count >= _Root.AllValues.Count)
			{
				return _Root.AllValues.Contains(item);
			}

			foreach (var name in item.Names)
			{
				var node = _Root;
				for (var i = 0; i < name.Parts.Count; ++i)
				{
					if (!node.TryGetEdge(name.Parts[i], out node))
					{
						break;
					}
					if (i == name.Parts.Count - 1 && node.Contains(item))
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
			=> _Root.AllValues.GetEnumerator();

		public int Remove(IImmutableCommand item)
		{
			var removed = 0;
			foreach (var name in item.Names)
			{
				var node = _Root;
				for (var i = 0; i < name.Parts.Count; ++i)
				{
					if (!node.TryGetEdge(name.Parts[i], out node))
					{
						break;
					}
					if (i == name.Parts.Count - 1 && node.Remove(item))
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
			private readonly HashSet<IImmutableCommand> _Commands;
			private readonly HashSet<IImmutableCommand> _DirectCommands;
			private readonly Dictionary<string, Node> _Edges;
			private readonly string? _Key;
			private readonly Node? _Parent;

			public IReadOnlyCollection<IImmutableCommand> AllValues => _Commands;
			public IReadOnlyCollection<IImmutableCommand> DirectValues => _DirectCommands;
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
					return $"Path = {path}, Count = {AllValues.Count}";
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
				_Commands = new HashSet<IImmutableCommand>();
				_DirectCommands = new HashSet<IImmutableCommand>();
				_Edges = new Dictionary<string, Node>(stringComparer);
				_Key = key;
				_Parent = parent;
			}

			public bool Add(IImmutableCommand command)
			{
				if (!_DirectCommands.Add(command))
				{
					return false;
				}

				// Add the command to all parent nodes
				for (var node = this; node is not null; node = node._Parent)
				{
					node._Commands.Add(command);
				}

				return true;
			}

			public bool Contains(IImmutableCommand command)
				// Only direct commands since the node has already been found via name
				=> _DirectCommands.Contains(command);

			public bool Remove(IImmutableCommand command)
			{
				if (!_DirectCommands.Remove(command))
				{
					return false;
				}

				// Remove the command from all parent nodes and kill all empty nodes
				for (var node = this; node is not null; node = node._Parent)
				{
					node._Commands.Remove(command);
					if (node._Commands.Count == 0 && node._Edges.Count == 0)
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
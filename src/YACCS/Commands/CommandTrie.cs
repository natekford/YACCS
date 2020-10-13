using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class CommandTrie : ICollection<IImmutableCommand>
	{
		private readonly IEqualityComparer<string> _StringComparer;

		public bool IsReadOnly => false;
		public Node Root { get; private set; }
		public int Count => Root.AllValues.Count;
		private string DebuggerDisplay => $"Count = {Count}";

		public CommandTrie(IEqualityComparer<string> stringComparer)
		{
			_StringComparer = stringComparer;
			Root = new Node(null!, null!, _StringComparer);
		}

		public int Add(IImmutableCommand item)
		{
			var added = 0;
			foreach (var name in item.Names)
			{
				var node = Root;
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
			=> Root = new Node(null!, null!, _StringComparer);

		public bool Contains(IImmutableCommand item)
		{
			foreach (var name in item.Names)
			{
				var node = Root;
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
			foreach (var command in Root.AllValues)
			{
				array[arrayIndex++] = command;
			}
		}

		public IEnumerator<IImmutableCommand> GetEnumerator()
			=> Root.AllValues.GetEnumerator();

		public int Remove(IImmutableCommand item)
		{
			var removed = 0;
			foreach (var name in item.Names)
			{
				var node = Root;
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
		public sealed class Node
		{
			private readonly HashSet<IImmutableCommand> _Commands;
			private readonly HashSet<IImmutableCommand> _DirectCommands;
			private readonly Dictionary<string, Node> _Edges;
			private readonly string _Key;
			private readonly Node _Parent;

			public IReadOnlyCollection<IImmutableCommand> AllValues => _Commands;
			public IReadOnlyCollection<IImmutableCommand> DirectValues => _DirectCommands;
			public IReadOnlyCollection<Node> Edges => _Edges.Values;
			private string DebuggerDisplay
			{
				get
				{
					var path = "";
					var node = this;
					while (node is not null)
					{
						path = node._Key + " " + path;
						node = node._Parent;
					}
					return $"Path = {path}, Count = {AllValues.Count}";
				}
			}

			public Node this[string key]
			{
				get => _Edges[key];
				internal set => _Edges[key] = value;
			}

			public Node(
				string key,
				Node parent,
				IEqualityComparer<string> stringComparer)
			{
				_Commands = new HashSet<IImmutableCommand>();
				_DirectCommands = new HashSet<IImmutableCommand>();
				_Edges = new Dictionary<string, Node>(stringComparer);
				_Key = key;
				_Parent = parent;
			}

			public bool TryGetEdge(string key, out Node node)
				=> _Edges.TryGetValue(key, out node);

			internal bool Add(IImmutableCommand command)
			{
				if (!_DirectCommands.Add(command))
				{
					return false;
				}

				// Add the command to all parent nodes
				var node = this;
				while (node is not null)
				{
					node._Commands.Add(command);
					node = node._Parent;
				}

				return true;
			}

			internal bool Contains(IImmutableCommand command)
				=> _Commands.Contains(command);

			internal bool Remove(IImmutableCommand command)
			{
				if (!_DirectCommands.Remove(command))
				{
					return false;
				}

				// Kill all empty nodes
				var node = this;
				while (node is not null)
				{
					node._Commands.Remove(command);
					if (node._Commands.Count == 0 && node._Edges.Count == 0 && node._Parent is not null)
					{
						node._Parent._Edges.Remove(node._Key);
					}
					node = node._Parent;
				}

				return true;
			}
		}
	}
}
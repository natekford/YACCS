using System;
using System.Collections;
using System.Collections.Generic;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public sealed class CommandTrie : ICollection<IImmutableCommand>
	{
		private readonly Dictionary<string, IImmutableCommand> _Commands;
		private readonly IEqualityComparer<string> _Comparer;

		public bool IsReadOnly => false;
		public Node Root { get; private set; }

		public int Count => _Commands.Count;

		public CommandTrie(IEqualityComparer<string> comparer)
		{
			_Commands = new Dictionary<string, IImmutableCommand>();
			_Comparer = comparer;
			Root = new Node(null!, null!, _Comparer);
		}

		public int Add(IImmutableCommand item)
		{
			if (!_Commands.TryAdd(item.PrimaryId, item))
			{
				throw new ArgumentException("Duplicate command id.", nameof(item));
			}

			var added = 0;
			foreach (var name in item.Names)
			{
				var node = Root;
				for (var i = 0; i < name.Parts.Count; ++i)
				{
					var key = name.Parts[i];
					if (!node.TryGetEdge(key, out var next))
					{
						node[key] = next = new Node(node, key, _Comparer);
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
		{
			_Commands.Clear();
			Root = new Node(null!, null!, _Comparer);
		}

		public bool Contains(IImmutableCommand item)
			=> _Commands.ContainsKey(item.PrimaryId);

		public void CopyTo(IImmutableCommand[] array, int arrayIndex)
			=> _Commands.Values.CopyTo(array, arrayIndex);

		public IEnumerator<IImmutableCommand> GetEnumerator()
			=> _Commands.Values.GetEnumerator();

		public int Remove(IImmutableCommand item)
		{
			if (_Commands.TryGetValue(item.PrimaryId, out var temp) && temp != item)
			{
				throw new ArgumentException("Attempted to remove an id which exists, but belongs to a different command.", nameof(item));
			}
			if (!_Commands.Remove(item.PrimaryId))
			{
				return 0;
			}

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

		public sealed class Node
		{
			private readonly Dictionary<string, IImmutableCommand> _Commands;
			private readonly Dictionary<string, Node> _Edges;
			private readonly string _Key;
			private readonly Node _Parent;
			public IReadOnlyCollection<Node> Edges => _Edges.Values;
			public IReadOnlyCollection<IImmutableCommand> Values => _Commands.Values;

			public Node this[string key]
			{
				get => _Edges[key];
				internal set => _Edges[key] = value;
			}

			public Node(Node parent, string key, IEqualityComparer<string> comparer)
			{
				_Parent = parent;
				_Key = key;
				_Edges = new Dictionary<string, Node>(comparer);
				_Commands = new Dictionary<string, IImmutableCommand>();
			}

			public IReadOnlyCollection<IImmutableCommand> GetCommands()
			{
				static IEnumerable<IImmutableCommand> GetCommandsEnumerable(Node node)
				{
					foreach (var command in node.Values)
					{
						yield return command;
					}
					foreach (var edge in node.Edges)
					{
						foreach (var command in GetCommandsEnumerable(edge))
						{
							yield return command;
						}
					}
				}

				// I should probably figure out how to not have to enumerate all child nodes
				var dict = new Dictionary<string, IImmutableCommand>();
				foreach (var command in GetCommandsEnumerable(this))
				{
					dict[command.PrimaryId] = command;
				}
				return dict.Values;
			}

			public bool TryGetEdge(string key, out Node node)
				=> _Edges.TryGetValue(key, out node);

			internal bool Add(IImmutableCommand command)
				=> _Commands.TryAdd(command.PrimaryId, command);

			internal bool Remove(IImmutableCommand command)
			{
				if (!_Commands.Remove(command.PrimaryId))
				{
					return false;
				}

				// Kill all empty nodes
				var node = this;
				while (node._Parent is not null)
				{
					if (node._Commands.Count == 0 && node._Edges.Count == 0)
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
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public sealed class CommandTrie : ITrie<string, IImmutableCommand>
	{
		private readonly ICommandServiceConfig _Config;
		private readonly ConcurrentDictionary<IImmutableCommand, byte> _Items;
		private readonly IReadOnlyDictionary<Type, ITypeReader> _Readers;
		private readonly Node _Root;

		public bool IsReadOnly => false;
		public IReadOnlyCollection<IImmutableCommand> Items
			=> (IReadOnlyCollection<IImmutableCommand>)_Items.Keys;
		public INode<string, IImmutableCommand> Root => _Root;
		public int Count => _Items.Count;
		private string DebuggerDisplay => $"Count = {Count}";

		public CommandTrie(
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ICommandServiceConfig config)
		{
			_Config = config;
			_Items = new();
			_Readers = readers;
			_Root = new(null, null, _Config.CommandNameComparer);
		}

		public int Add(IImmutableCommand item)
		{
			// Commands cannot be added directly to ROOT
			if (item.Names.Count == 0)
			{
				throw new ArgumentException("Cannot add a command with no name.", nameof(item));
			}

			// Verify that every name is valid
			foreach (var name in item.Names)
			{
				foreach (var part in name)
				{
					if (part.Contains(_Config.Separator))
					{
						throw new ArgumentException($"'{name}' cannot contain the separator character.", nameof(item));
					}
				}
			}

			// Verify that every parameter has a type reader and that the reader can accept
			// the context that the command accepts
			foreach (var parameter in item.Parameters)
			{
				ITypeReader reader;
				try
				{
					reader = _Readers.GetTypeReader(parameter);
				}
				catch (Exception e)
				{
					throw new ArgumentException("Unregistered type reader for " +
						$"'{parameter.ParameterName}' from '{item.Names?.FirstOrDefault()}'.",
						nameof(item), e);
				}

				// If A can't inherit B and B can't inherit A then neither is part of the
				// same inheritance chain and there will never be a valid context
				if (!reader.ContextType.IsAssignableFrom(item.ContextType) &&
					!item.ContextType.IsAssignableFrom(reader.ContextType))
				{
					throw new ArgumentException("Invalid type reader for " +
						$"'{parameter.ParameterName}' from '{item.Names?.FirstOrDefault()}'. " +
						$"Type reader accepts '{reader.ContextType}', " +
						$"command accepts '{item.ContextType}'. " +
						"The type reader will never receive a valid context.", nameof(item));
				}
			}

			var added = 0;
			if (_Items.TryAdd(item, 0))
			{
				foreach (var name in item.Names)
				{
					var node = _Root;
					for (var i = 0; i < name.Count; ++i)
					{
						var key = name[i];
						if (!node.TryGetEdge(key, out var next))
						{
							node[key] = next = new(key, node, _Config.CommandNameComparer);
						}
						if (i == name.Count - 1 && next.Add(item))
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

		public bool Contains(IImmutableCommand item)
		{
			if (item.Names.Count >= _Items.Count)
			{
				return _Items.ContainsKey(item);
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
			=> Items.GetEnumerator();

		public int Remove(IImmutableCommand item)
		{
			var removed = 0;
			if (_Items.TryRemove(item, out _))
			{
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
			}
			return removed;
		}

		void ICollection<IImmutableCommand>.Add(IImmutableCommand item)
			=> Add(item);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		bool ICollection<IImmutableCommand>.Remove(IImmutableCommand item)
			=> Remove(item) > 0;

		[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
		private sealed class Node : INode<string, IImmutableCommand>
		{
			private readonly ConcurrentDictionary<string, Node> _Edges;
			private readonly ConcurrentDictionary<IImmutableCommand, byte> _Items;
			private readonly string? _Key;
			private readonly Node? _Parent;

			public IReadOnlyCollection<IImmutableCommand> Items
				=> (IReadOnlyCollection<IImmutableCommand>)_Items.Keys;
			public IReadOnlyCollection<Node> Edges
				=> (IReadOnlyCollection<Node>)_Edges.Values;
			IReadOnlyCollection<INode<string, IImmutableCommand>> INode<string, IImmutableCommand>.Edges
				=> Edges;
			private string DebuggerDisplay
			{
				get
				{
					var path = string.Empty;
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
			INode<string, IImmutableCommand> INode<string, IImmutableCommand>.this[string key]
				=> this[key];

			public Node(string? key, Node? parent, IEqualityComparer<string> stringComparer)
			{
				_Edges = new(stringComparer);
				_Items = new();
				_Key = key;
				_Parent = parent;
			}

			public bool Add(IImmutableCommand item)
				=> _Items.TryAdd(item, 0);

			public void Clear()
			{
				_Edges.Clear();
				_Items.Clear();
			}

			public bool Contains(IImmutableCommand item)
				// Only direct commands since the node has already been found via name
				=> _Items.ContainsKey(item);

			public bool Remove(IImmutableCommand item)
			{
				var isRemoved = _Items.TryRemove(item, out _);
				if (isRemoved)
				{
					// Kill all empty nodes
					for (var node = this; node is not null; node = node._Parent)
					{
						if (node._Items.Count == 0 && node._Edges.Count == 0)
						{
							node._Parent?._Edges?.TryRemove(node._Key!, out _);
						}
					}
				}
				return isRemoved;
			}

			public bool TryGetEdge(string key, [NotNullWhen(true)] out Node? node)
				=> _Edges.TryGetValue(key, out node);

			bool INode<string, IImmutableCommand>.TryGetEdge(
				string key,
				[NotNullWhen(true)] out INode<string, IImmutableCommand>? node)
			{
				var result = TryGetEdge(key, out var temp);
				node = temp!;
				return result;
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public class CommandTrie
	{
		private readonly Dictionary<string, ICommand> _Commands;
		private readonly IEqualityComparer<string> _Comparer;

		public Node Root { get; }

		public CommandTrie(IEqualityComparer<string> comparer)
		{
			_Commands = new Dictionary<string, ICommand>();
			_Comparer = comparer;
			Root = new Node(_Comparer);
		}

		public int Add(ICommand command)
		{
			if (!_Commands.TryAdd(command.Id, command))
			{
				return 0;
			}

			var added = 0;
			foreach (var name in command.Names)
			{
				var node = Root;
				for (var i = 0; i < name.Parts.Count; ++i)
				{
					var part = name.Parts[i];
					if (!node.Edges.TryGetValue(part, out var next))
					{
						next = new Node(_Comparer);
						node.MutableEdges.Add(part, next);
					}
					if (i == name.Parts.Count - 1 && !next.MutableValues.Contains(command))
					{
						next.MutableValues.Add(command);
						++added;
					}
					node = next;
				}
			}
			return added;
		}

		public IReadOnlyList<ICommand> GetCommands()
			=> _Commands.Values.ToImmutableArray();

		public IReadOnlyList<ICommand> GetCommands(Node node)
		{
			var set = new HashSet<string>();
			var array = ImmutableArray.CreateBuilder<ICommand>();
			foreach (var command in GetCommandsEnumerable(node))
			{
				if (!set.Add(command.Id))
				{
					continue;
				}
				array.Add(command);
			}
			return array.MoveToImmutable();
		}

		public int Remove(ICommand command)
		{
			if (!_Commands.Remove(command.Id))
			{
				return 0;
			}

			var removed = 0;
			foreach (var name in command.Names)
			{
				var node = Root;
				for (var i = 0; i < name.Parts.Count; ++i)
				{
					var part = name.Parts[i];
					if (!node.Edges.TryGetValue(part, out var next))
					{
						break;
					}
					if (i == name.Parts.Count - 1 && next.MutableValues.Remove(command))
					{
						++removed;
					}
					node = next;
				}
			}
			return removed;
		}

		private IEnumerable<ICommand> GetCommandsEnumerable(Node node)
		{
			foreach (var command in node.Values)
			{
				yield return command;
			}
			foreach (var edge in node.Edges.Values)
			{
				foreach (var command in GetCommands(edge))
				{
					yield return command;
				}
			}
		}

		public sealed class Node
		{
			public IReadOnlyDictionary<string, Node> Edges => MutableEdges;
			public IReadOnlyList<ICommand> Values => MutableValues;
			internal Dictionary<string, Node> MutableEdges { get; }
			internal List<ICommand> MutableValues { get; }

			public Node(IEqualityComparer<string> comparer)
			{
				MutableEdges = new Dictionary<string, Node>(comparer);
				MutableValues = new List<ICommand>();
			}
		}
	}
}
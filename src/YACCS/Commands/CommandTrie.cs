using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public class CommandTrie
	{
		private readonly Dictionary<string, IImmutableCommand> _Commands;
		private readonly IEqualityComparer<string> _Comparer;

		public Node Root { get; }

		public CommandTrie(IEqualityComparer<string> comparer)
		{
			_Commands = new Dictionary<string, IImmutableCommand>();
			_Comparer = comparer;
			Root = new Node(_Comparer);
		}

		public int Add(IImmutableCommand command)
		{
			if (!_Commands.TryAdd(command.PrimaryId, command))
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

		public IReadOnlyList<IImmutableCommand> GetCommands()
			=> _Commands.Values.ToImmutableArray();

		public IReadOnlyList<IImmutableCommand> GetCommands(Node node)
		{
			var set = new HashSet<string>();
			var array = ImmutableArray.CreateBuilder<IImmutableCommand>();
			foreach (var command in GetCommandsEnumerable(node))
			{
				if (!set.Add(command.PrimaryId))
				{
					continue;
				}
				array.Add(command);
			}
			return array.MoveToImmutable();
		}

		public int Remove(IImmutableCommand command)
		{
			if (!_Commands.Remove(command.PrimaryId))
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

		private IEnumerable<IImmutableCommand> GetCommandsEnumerable(Node node)
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
			public IReadOnlyList<IImmutableCommand> Values => MutableValues;
			internal Dictionary<string, Node> MutableEdges { get; }
			internal List<IImmutableCommand> MutableValues { get; }

			public Node(IEqualityComparer<string> comparer)
			{
				MutableEdges = new Dictionary<string, Node>(comparer);
				MutableValues = new List<IImmutableCommand>();
			}
		}
	}
}
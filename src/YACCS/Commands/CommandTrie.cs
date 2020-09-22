using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public class CommandTrie
	{
		private readonly Dictionary<string, ICommand> _Commands = new Dictionary<string, ICommand>();
		public Node Root { get; } = new Node();

		public CommandTrie()
		{
		}

		public CommandTrie(IEnumerable<ICommand> commands)
		{
			foreach (var command in commands)
			{
				Add(command);
			}
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
						next = new Node();
						node.Edges.Add(part, next);
					}
					if (i == name.Parts.Count - 1 && !next.Values.Contains(command))
					{
						next.Values.Add(command);
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
					if (i == name.Parts.Count - 1 && next.Values.Remove(command))
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
			public IDictionary<string, Node> Edges { get; }
				= new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
			public IList<ICommand> Values { get; } = new List<ICommand>();
		}
	}
}
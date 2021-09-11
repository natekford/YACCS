using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.Trie;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	/// <summary>
	/// A <see cref="Trie{TKey, TValue}"/> specifically for commands.
	/// </summary>
	public sealed class CommandTrie
		: Trie<string, IImmutableCommand>, ICommandCollection<IImmutableCommand>
	{
		private readonly IReadOnlyDictionary<Type, ITypeReader> _Readers;
		private readonly char _Separator;

		/// <summary>
		/// Creates a new <see cref="CommandTrie"/>.
		/// </summary>
		/// <param name="readers">
		/// The type readers to search through when determining if each parameter has a
		/// registered type reader.
		/// </param>
		/// <param name="separator">The character which cannot be directly in any path.</param>
		/// <param name="comparer">The comparer to use when comparing keys.</param>
		public CommandTrie(
			IReadOnlyDictionary<Type, ITypeReader> readers,
			char separator,
			IEqualityComparer<string> comparer)
			: base(comparer)
		{
			_Separator = separator;
			_Readers = readers;
		}

		/// <inheritdoc />
		/// <exception cref="ArgumentException">
		/// When <paramref name="item"/> contains a path with the separator character
		/// - or -
		/// When <paramref name="item"/> contains a parameter which does not have a value for
		/// <see cref="IImmutableParameter.TypeReader"/> and <see cref="IQueryableParameter.ParameterType"/>
		/// does not have a registered type reader.
		/// - or -
		/// When <paramref name="item"/> contains a parameter where its type reader context
		/// type and <paramref name="item"/>'s context type can never be the same type.
		/// </exception>
		public override void Add(IImmutableCommand item)
		{
			// Commands cannot be added directly to ROOT
			if (item.Paths.Count == 0)
			{
				throw new ArgumentException("Cannot add a command with no name.", nameof(item));
			}

			// Verify that every name is valid
			foreach (var path in item.Paths)
			{
				foreach (var part in path)
				{
					if (part.Contains(_Separator))
					{
						throw new ArgumentException($"'{path}' cannot contain the separator character.", nameof(item));
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
						$"'{parameter.ParameterName}' from '{item.Paths?.FirstOrDefault()}'.",
						nameof(item), e);
				}

				// If A can't inherit B and B can't inherit A then neither is part of the
				// same inheritance chain and there will never be a valid context
				if (!reader.ContextType.IsAssignableFrom(item.ContextType) &&
					!item.ContextType.IsAssignableFrom(reader.ContextType))
				{
					throw new ArgumentException("Invalid type reader for " +
						$"'{parameter.ParameterName}' from '{item.Paths?.FirstOrDefault()}'. " +
						$"Type reader accepts '{reader.ContextType}', " +
						$"command accepts '{item.ContextType}'. " +
						"The type reader will never receive a valid context.", nameof(item));
				}
			}

			base.Add(item);
		}

		/// <inheritdoc />
		public IReadOnlyCollection<IImmutableCommand> Find(ReadOnlyMemory<string> path)
		{
			var node = Root;
			foreach (var key in path.Span)
			{
				if (!node.TryGetEdge(key, out node))
				{
					break;
				}
			}
			if (node is null)
			{
				return Array.Empty<IImmutableCommand>();
			}

			// Generated items have a source and that source gives them the same
			// names/properties, so they should be ignored since they are copies
			return node.GetAllDistinctItems(x => x.Source is null);
		}

		/// <inheritdoc />
		public IEnumerable<WithDepth<IImmutableCommand>> Iterate(ReadOnlyMemory<string> path)
		{
			var node = Root;
			for (var i = 0; i < path.Length; ++i)
			{
				if (!node.TryGetEdge(path.Span[i], out node))
				{
					yield break;
				}
				foreach (var command in node.Items)
				{
					yield return new(i, command);
				}
			}
		}

		/// <inheritdoc />
		protected override IEnumerable<IReadOnlyList<string>> GetPaths(IImmutableCommand item)
			=> item.Paths;
	}
}
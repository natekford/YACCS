using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public sealed class CommandTrie : Trie<string, IImmutableCommand>
	{
		private readonly ICommandServiceConfig _Config;
		private readonly IReadOnlyDictionary<Type, ITypeReader> _Readers;

		public CommandTrie(
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ICommandServiceConfig config)
			: base(config.CommandNameComparer)
		{
			_Config = config;
			_Readers = readers;
		}

		/// <inheritdoc />
		public override void Add(IImmutableCommand item)
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

			base.Add(item);
		}

		/// <inheritdoc />
		protected override IEnumerable<IReadOnlyList<string>> GetPaths(IImmutableCommand item)
			=> item.Names;
	}
}
using YACCS.Commands.Models;
using YACCS.Trie;
using YACCS.TypeReaders;

namespace YACCS.Commands;

/// <summary>
/// A <see cref="Trie{TKey, TValue}"/> specifically for commands.
/// </summary>
/// <remarks>
/// Creates a new <see cref="CommandTrie"/>.
/// </remarks>
/// <param name="readers">
/// The type readers to search through when determining if each parameter has a
/// registered type reader.
/// </param>
/// <param name="separator">The character which cannot be directly in any path.</param>
/// <param name="comparer">The comparer to use when comparing keys.</param>
public sealed class CommandTrie(
	IReadOnlyDictionary<Type, ITypeReader> readers,
	char separator,
	IEqualityComparer<string> comparer
) : Trie<string, IImmutableCommand>(comparer)
{
	private readonly IReadOnlyDictionary<Type, ITypeReader> _Readers = readers;
	private readonly char _Separator = separator;

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
	protected override IEnumerable<IReadOnlyList<string>> GetPaths(IImmutableCommand item)
		=> item.Paths;
}
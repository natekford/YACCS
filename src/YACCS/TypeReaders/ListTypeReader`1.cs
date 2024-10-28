using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a <see cref="List{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListTypeReader<T> : CollectionTypeReader<T, List<T>>
{
	/// <inheritdoc />
	protected override ValueTask<List<T>> CreateCollectionAsync(
		IContext context,
		ReadOnlyMemory<string> input)
		=> new(new List<T>(input.Length));
}
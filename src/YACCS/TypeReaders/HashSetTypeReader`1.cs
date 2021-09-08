using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// Parses a <see cref="HashSet{T}"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class HashSetTypeReader<T> : CollectionTypeReader<T, HashSet<T>>
	{
		/// <inheritdoc />
		protected override ValueTask<HashSet<T>> CreateCollectionAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> new(new HashSet<T>(input.Length));
	}
}
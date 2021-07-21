using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class HashSetTypeReader<T> : CollectionTypeReader<T, HashSet<T>>
	{
		protected override ValueTask<HashSet<T>> CreateCollectionAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> new(new HashSet<T>(input.Length));
	}
}
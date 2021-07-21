﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class ListTypeReader<T> : CollectionTypeReader<T, List<T>>
	{
		protected override ValueTask<List<T>> CreateCollectionAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> new(new List<T>(input.Length));
	}
}
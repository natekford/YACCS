using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public sealed class AggregateTypeReader<T> : TypeReader<IContext, T>
	{
		private readonly ImmutableArray<ITypeReader<T>> _Readers;

		public AggregateTypeReader(IEnumerable<ITypeReader> readers)
		{
			_Readers = readers.Cast<ITypeReader<T>>().ToImmutableArray();
		}

		public override async ITask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			foreach (var reader in _Readers)
			{
				var result = await reader.ReadAsync(context, input).ConfigureAwait(false);
				if (result.InnerResult.IsSuccess)
				{
					return result;
				}
			}
			return CachedResults<T>.ParseFailed.Result;
		}
	}
}
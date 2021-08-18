using System;
using System.Collections.Concurrent;

using YACCS.TypeReaders;

namespace YACCS.Results
{
	public sealed class TypeReaderResultCache<T>
	{
		private readonly ConcurrentDictionary<string, ITypeReaderResult<T>> _Cache = new();
		private readonly Func<string, IResult> _Factory;

		public ITypeReaderResult<T> this[string key]
			=> _Cache.GetOrAdd(key, (x, f) => TypeReaderResult<T>.FromError(f(x)), _Factory);

		public TypeReaderResultCache(Func<string, IResult> factory)
		{
			_Factory = factory;
		}
	}
}
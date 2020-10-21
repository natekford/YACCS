using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public delegate ITask<ITypeReaderResult<T>> TypeReaderCacheDelegate<T>(IContext context, string input);

	public class TypeReaderCache : ITypeReaderCache
	{
		private readonly Dictionary<Guid, Dictionary<TRKey, ITask<ITypeReaderResult>>> _Cache
			= new Dictionary<Guid, Dictionary<TRKey, ITask<ITypeReaderResult>>>();

		public ITask<ITypeReaderResult<T>> GetAsync<T>(
			ITypeReader<T> reader,
			IContext context,
			string input,
			TypeReaderCacheDelegate<T> func)
		{
			if (!_Cache.TryGetValue(context.Id, out var dict))
			{
				_Cache[context.Id] = dict = new Dictionary<TRKey, ITask<ITypeReaderResult>>();
			}
			var key = new TRKey(reader, input);
			if (dict.TryGetValue(key, out var result))
			{
				return (ITask<ITypeReaderResult<T>>)result;
			}

			var task = func.Invoke(context, input);
			dict[key] = task;
			return task;
		}

		public void Remove(IContext context)
			=> _Cache.Remove(context.Id);

		private readonly struct TRKey : IEquatable<TRKey>
		{
			public ITypeReader TypeReader { get; }
			public string Value { get; }

			public TRKey(ITypeReader typeReader, string value)
			{
				TypeReader = typeReader;
				Value = value;
			}

			public bool Equals(TRKey other)
				=> TypeReader == other.TypeReader && Value.Equals(other.Value);
		}
	}
}
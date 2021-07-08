using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS
{
	public abstract class TypeRegistry<T> : IReadOnlyDictionary<Type, T>
	{
		public int Count => Items.Count;
		public IEnumerable<Type> Keys => Items.Keys;
		public IEnumerable<T> Values => Items.Values;
		protected abstract IDictionary<Type, T> Items { get; }

		public virtual T this[Type key]
			=> TryGetValue(key, out var value) ? value : Items[key];

		public bool ContainsKey(Type key)
			=> Items.ContainsKey(key);

		public IEnumerator<KeyValuePair<Type, T>> GetEnumerator()
			=> Items.GetEnumerator();

		public abstract bool TryGetValue(Type key, [NotNullWhen(true)] out T value);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
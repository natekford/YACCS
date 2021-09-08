using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS
{
	public abstract class TypeRegistry<T> : IReadOnlyDictionary<Type, T>
	{
		/// <inheritdoc />
		public int Count => Items.Count;
		/// <inheritdoc />
		public IEnumerable<Type> Keys => Items.Keys;
		/// <inheritdoc />
		public IEnumerable<T> Values => Items.Values;
		/// <inheritdoc />
		protected abstract IDictionary<Type, T> Items { get; }

		/// <inheritdoc />
		public virtual T this[Type key]
			=> TryGetValue(key, out var value) ? value : Items[key];

		/// <inheritdoc />
		public bool ContainsKey(Type key)
			=> Items.ContainsKey(key);

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<Type, T>> GetEnumerator()
			=> Items.GetEnumerator();

		/// <inheritdoc />
		public abstract bool TryGetValue(Type key, [NotNullWhen(true)] out T value);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
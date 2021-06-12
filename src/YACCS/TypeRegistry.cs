using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS
{
	public abstract class TypeRegistry<T> : IDictionary<Type, T>, IReadOnlyDictionary<Type, T>
	{
		public int Count => Items.Count;
		public bool IsReadOnly => Items.IsReadOnly;
		public ICollection<Type> Keys => Items.Keys;
		IEnumerable<Type> IReadOnlyDictionary<Type, T>.Keys => Keys;
		public ICollection<T> Values => Items.Values;
		IEnumerable<T> IReadOnlyDictionary<Type, T>.Values => Values;
		protected IDictionary<Type, T> Items { get; }

		public virtual T this[Type key]
		{
			get => TryGetValue(key, out var value) ? value : Items[key];
			set => Items[key] = value;
		}

		protected TypeRegistry(IDictionary<Type, T> items)
		{
			Items = items;
		}

		public void Add(Type key, T value)
			=> Items.Add(key, value);

		public void Add(KeyValuePair<Type, T> item)
			=> Add(item.Key, item.Value);

		public void Clear()
			=> Items.Clear();

		public bool Contains(KeyValuePair<Type, T> item)
			=> Items.Contains(item);

		public bool ContainsKey(Type key)
			=> Items.ContainsKey(key);

		public void CopyTo(KeyValuePair<Type, T>[] array, int arrayIndex)
			=> Items.CopyTo(array, arrayIndex);

		public IEnumerator<KeyValuePair<Type, T>> GetEnumerator()
			=> Items.GetEnumerator();

		public bool Remove(Type key)
			=> Items.Remove(key);

		public bool Remove(KeyValuePair<Type, T> item)
			=> Items.Remove(item);

		public abstract bool TryGetValue(Type key, [NotNullWhen(true)] out T value);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
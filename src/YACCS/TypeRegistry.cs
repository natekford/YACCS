using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace YACCS;

/// <summary>
/// The base class for a registry of <typeparamref name="T"/> with a key of <see cref="Type"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class TypeRegistry<T> : IReadOnlyDictionary<Type, T>
{
	/// <inheritdoc />
	public int Count => Items.Count;
	/// <inheritdoc />
	public IEnumerable<Type> Keys => Items.Keys;
	/// <inheritdoc />
	public IEnumerable<T> Values => Items.Values;
	/// <summary>
	/// The dictionary this registry is wrapping.
	/// </summary>
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

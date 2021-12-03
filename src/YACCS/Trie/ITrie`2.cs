namespace YACCS.Trie;

/// <summary>
/// A <a href="https://en.wikipedia.org/wiki/Trie">trie</a> data structure.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public interface ITrie<TKey, TValue> : IReadOnlyTrie<TKey, TValue>, ICollection<TValue>
{
	/// <inheritdoc cref="ICollection{T}.Count"/>
	new int Count { get; }
}

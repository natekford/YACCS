namespace YACCS.Trie
{
	/// <summary>
	/// A read-only <a href="https://en.wikipedia.org/wiki/Trie">trie</a> data structure.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface IReadOnlyTrie<TKey, TValue> : IReadOnlyCollection<TValue>
	{
		/// <summary>
		/// The root element of this trie.
		/// </summary>
		INode<TKey, TValue> Root { get; }
	}
}
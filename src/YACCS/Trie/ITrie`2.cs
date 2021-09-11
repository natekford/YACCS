using System.Collections.Generic;

namespace YACCS.Trie
{
	/// <summary>
	/// A <a href="https://en.wikipedia.org/wiki/Trie">trie</a> data structure.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface ITrie<TKey, TValue> : IReadOnlyCollection<TValue>, ICollection<TValue>
	{
		/// <inheritdoc cref="ICollection{T}.Count"/>
		new int Count { get; }
		/// <summary>
		/// All items added to this trie, regardless of their positions.
		/// </summary>
		IReadOnlyCollection<TValue> Items { get; }
		/// <summary>
		/// The root element of this trie.
		/// </summary>
		INode<TKey, TValue> Root { get; }
	}
}
using System.Collections.Generic;

namespace YACCS.Commands
{
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
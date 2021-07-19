using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Commands
{
	public interface INode<TKey, TValue>
	{
		IReadOnlyCollection<INode<TKey, TValue>> Edges { get; }
		IReadOnlyCollection<TValue> Items { get; }
		INode<TKey, TValue> this[TKey key] { get; }

		bool TryGetEdge(TKey key, [NotNullWhen(true)] out INode<TKey, TValue>? node);
	}
}
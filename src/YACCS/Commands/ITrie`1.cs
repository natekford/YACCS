using System.Collections.Generic;

namespace YACCS.Commands
{
	public interface ITrie<TKey, TValue> : IReadOnlyCollection<TValue>, ICollection<TValue>
	{
		new int Count { get; }
		INode<TKey, TValue> Root { get; }
	}
}
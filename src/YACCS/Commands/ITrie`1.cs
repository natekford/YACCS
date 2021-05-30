using System.Collections.Generic;

namespace YACCS.Commands
{
	public interface ITrie<T> : IReadOnlyCollection<T>, ICollection<T>
	{
		new int Count { get; }
		INode<T> Root { get; }
	}
}
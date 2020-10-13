using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Commands
{
	public interface INode<T>
	{
		IReadOnlyCollection<T> AllValues { get; }
		IReadOnlyCollection<T> DirectValues { get; }
		IReadOnlyCollection<INode<T>> Edges { get; }

		INode<T> this[string key] { get; }

		bool TryGetEdge(string key, [NotNullWhen(true)] out INode<T>? node);
	}

	public interface ITrie<T> : IReadOnlyCollection<T>, ICollection<T>
	{
		new int Count { get; }
		INode<T> Root { get; }
	}
}
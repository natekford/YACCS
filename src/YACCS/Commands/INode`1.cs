using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Commands
{
	public interface INode<T>
	{
		IReadOnlyCollection<INode<T>> Edges { get; }
		IReadOnlyCollection<T> Items { get; }
		INode<T> this[string key] { get; }

		bool TryGetEdge(string key, [NotNullWhen(true)] out INode<T>? node);
	}
}
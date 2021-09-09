﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Commands
{
	/// <summary>
	/// Defines the methods and properties of a node for a <see cref="ITrie{TKey, TValue}"/>.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface INode<TKey, TValue>
	{
		/// <summary>
		/// All the edges of this node.
		/// </summary>
		IReadOnlyCollection<INode<TKey, TValue>> Edges { get; }
		/// <summary>
		/// All the items that are directly inside this node.
		/// </summary>
		IReadOnlyCollection<TValue> Items { get; }
		/// <summary>
		/// Gets the edge that is the value of <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key for the edge.</param>
		/// <returns>The edge that has the specified <paramref name="key"/>.</returns>
		INode<TKey, TValue> this[TKey key] { get; }

		/// <summary>
		/// Tries to get an edge via <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key for the edge.</param>
		/// <param name="node">The edge that has the specified <paramref name="key"/>.</param>
		/// <returns>A bool indicating success or failure.</returns>
		bool TryGetEdge(TKey key, [NotNullWhen(true)] out INode<TKey, TValue>? node);
	}
}
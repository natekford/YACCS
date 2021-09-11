using System;
using System.Collections.Generic;

namespace YACCS.Commands
{
	/// <summary>
	/// Represents a collection of commands.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICommandCollection<T>
		: ICollection<T>, IReadOnlyCommandCollection<T>
	{
		/// <inheritdoc cref="ICollection{T}.Count" />
		new int Count { get; }

		/// <summary>
		/// Iterates through commands at each step of <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to traverse.</param>
		/// <returns>An enumerable of commands retrieved while traversing the path.</returns>
		IEnumerable<WithDepth<T>> Iterate(ReadOnlyMemory<string> path);
	}
}
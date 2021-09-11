using System;
using System.Collections.Generic;

namespace YACCS.Commands
{
	/// <summary>
	/// Represents a read-only collection of commands.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReadOnlyCommandCollection<T> : IReadOnlyCollection<T>
	{
		/// <inheritdoc cref="IReadOnlyCollection{T}.Count" />
		new int Count { get; }

		/// <summary>
		/// Finds a command via <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to search for.</param>
		/// <returns>A collection of matching commands.</returns>
		IReadOnlyCollection<T> Find(ReadOnlyMemory<string> path);
	}
}
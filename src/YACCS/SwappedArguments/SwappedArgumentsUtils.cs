using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.SwappedArguments
{
	/// <summary>
	/// Utilties for swapped arguments.
	/// </summary>
	public static class SwappedArgumentsUtils
	{
		/// <inheritdoc cref="GenerateSwappedArgumentsVersions(IImmutableCommand, IReadOnlyList{int}, int)"/>
		/// <exception cref="InvalidOperationException">
		/// When a parameter marked with a <see cref="IImmutableParameter.Length"/> of
		/// <see langword="null"/> is swapped.
		/// </exception>
		public static IEnumerable<SwappedArgumentsCommand> GenerateSwappedArgumentsVersions(
			this IImmutableCommand command,
			int priorityDifference)
		{
			var indices = new List<int>(command.Parameters.Count);
			for (var i = 0; i < command.Parameters.Count; ++i)
			{
				var parameter = command.Parameters[i];
				if (!parameter.GetAttributes<SwappableAttribute>().Any())
				{
					continue;
				}
				if (parameter.Length is null)
				{
					throw new InvalidOperationException(
						$"Cannot swap the parameter '{parameter.OriginalParameterName}' " +
						$"from '{command.Paths?.FirstOrDefault()}' because it is a remainder.");
				}
				indices.Add(i);
			}
			return command.GenerateSwappedArgumentsVersions(indices, priorityDifference);
		}

		/// <summary>
		/// Creates permutations for <paramref name="indices"/> and then returns
		/// <see cref="SwappedArgumentsCommand"/> for each one.
		/// </summary>
		/// <param name="command">The command which is being wrapped.</param>
		/// <param name="indices">The indices to swap around.</param>
		/// <param name="priorityDifference">
		/// The amount to lower priority by for each step taken in reordering the indices.
		/// </param>
		/// <returns>An enumerable of commands with swapped arguments.</returns>
		public static IEnumerable<SwappedArgumentsCommand> GenerateSwappedArgumentsVersions(
			this IImmutableCommand command,
			IReadOnlyList<int> indices,
			int priorityDifference)
		{
			foreach (var swapper in Swapper.CreateSwappers(indices))
			{
				yield return new(command, swapper, priorityDifference);
			}
		}
	}
}
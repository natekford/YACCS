﻿using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.SwappedArguments
{
	public static class SwappedArgumentsUtils
	{
		public static IEnumerable<SwappedArgumentsCommand> GenerateSwappedArgumentsVersions(
			this IImmutableCommand command,
			int priorityDifference)
		{
			var indices = new List<int>(command.Parameters.Count);
			for (var i = 0; i < command.Parameters.Count; ++i)
			{
				var parameter = command.Parameters[i];
				if (!parameter.Get<ISwappableAttribute>().Any())
				{
					continue;
				}
				if (parameter.Length is null)
				{
					throw new InvalidOperationException(
						$"Cannot swap the parameter '{parameter.OriginalParameterName}' " +
						$"from '{command.Names?.FirstOrDefault()}' because it is a remainder.");
				}

				indices.Add(i);
			}
			return command.GenerateSwappedArgumentsVersions(indices, priorityDifference);
		}

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
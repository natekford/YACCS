using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands.Building;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.SwappedArguments;

/// <summary>
/// Specifies that the applied command has arguments which can be swapped.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class GenerateSwappedArgumentsAttribute : Attribute, ICommandGeneratorAttribute
{
	/// <summary>
	/// The amount to lower priority by for each step taken in reordering the indices.
	/// </summary>
	public int PriorityDifference { get; set; } = 1;

	/// <inheritdoc />
	public ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
		IServiceProvider services,
		IImmutableCommand source)
		=> new(GenerateCommands(source));

	private IEnumerable<SwappedArgumentsCommand> GenerateCommands(IImmutableCommand source)
	{
		var indices = new List<int>(source.Parameters.Count);
		for (var i = 0; i < source.Parameters.Count; ++i)
		{
			var parameter = source.Parameters[i];
			if (!parameter.GetAttributes<SwappableAttribute>().Any())
			{
				continue;
			}
			if (parameter.Length is null)
			{
				throw new InvalidOperationException(
					$"Cannot swap the parameter '{parameter.OriginalParameterName}' " +
					$"from '{source.Paths?.FirstOrDefault()}' because it is a remainder.");
			}
			indices.Add(i);
		}

		foreach (var swapper in Swapper.CreateSwappers(indices))
		{
			yield return new(source, PriorityDifference * swapper.Swaps.Length, swapper);
		}
	}
}
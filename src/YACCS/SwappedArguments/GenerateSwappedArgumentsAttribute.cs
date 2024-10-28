using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Building;
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
		=> new(source.GenerateSwappedArgumentsVersions(PriorityDifference));
}
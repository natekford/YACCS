using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{
	/// <summary>
	/// Defines a method for generating commands.
	/// </summary>
	public interface ICommandGeneratorAttribute
	{
		/// <summary>
		/// Generates commands from <paramref name="source"/>.
		/// </summary>
		/// <param name="services">The services to use for dependency injection.</param>
		/// <param name="source">The command to generate commands from.</param>
		/// <returns>An enumerable of generated commands.</returns>
		ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
			IServiceProvider services,
			IImmutableCommand source);
	}
}
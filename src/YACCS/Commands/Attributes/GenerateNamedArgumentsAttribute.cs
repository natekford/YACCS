using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.NamedArguments;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GenerateNamedArgumentsAttribute : Attribute, ICommandGeneratorAttribute
	{
		public ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
			IServiceProvider services,
			IImmutableCommand original)
			=> new(new[] { new NamedArgumentsCommand(original) });
	}
}
using System;
using System.Collections.Generic;

using YACCS.Commands.Models;
using YACCS.NamedArguments;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GenerateNamedArgumentsAttribute : Attribute, ICommandGeneratorAttribute
	{
		public IEnumerable<IImmutableCommand> GenerateCommands(IImmutableCommand original)
			=> original.GenerateNamedArgumentVersion();
	}
}
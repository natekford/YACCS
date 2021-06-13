using System;
using System.Collections.Generic;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.SwapArguments
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GenerateSwappedArgumentsAttribute : Attribute, ICommandGeneratorAttribute
	{
		public int PriorityDifference { get; set; } = 1;

		public IEnumerable<IImmutableCommand> GenerateCommands(IImmutableCommand original)
			=> original.GenerateSwappedArgumentsVersions(PriorityDifference);
	}
}
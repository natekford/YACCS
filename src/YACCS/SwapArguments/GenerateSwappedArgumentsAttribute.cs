using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.SwapArguments
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GenerateSwappedArgumentsAttribute : Attribute, ICommandGeneratorAttribute
	{
		public int PriorityDifference { get; set; } = -1;

		public ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
			IServiceProvider services,
			IImmutableCommand original)
			=> new(original.GenerateSwappedArgumentsVersions(PriorityDifference));
	}
}
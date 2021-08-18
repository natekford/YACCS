using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.StructuredArguments
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class GenerateXmlArgumentsAttribute : Attribute, ICommandGeneratorAttribute
	{
		public ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
			IServiceProvider services,
			IImmutableCommand original)
			=> new(new[] { new XmlArgumentsCommand(original) });
	}
}
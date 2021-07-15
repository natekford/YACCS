using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.NamedArguments
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GenerateNamedArgumentsAttribute : Attribute, ICommandGeneratorAttribute, IParameterModifierAttribute
	{
		public ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
			IServiceProvider services,
			IImmutableCommand original)
			=> new(new[] { new NamedArgumentsCommand(original) });

		public void ModifyParameter(IParameter parameter)
		{
			var pType = parameter.ParameterType;
			var ppType = typeof(NamedArgumentsParameterPrecondition<>).MakeGenericType(pType);
			parameter.Attributes.Add(Activator.CreateInstance(ppType));
			parameter.MarkAsRemainder();
		}
	}
}
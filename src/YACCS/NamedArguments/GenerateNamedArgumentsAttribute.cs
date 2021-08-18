using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.NamedArguments
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class GenerateNamedArgumentsAttribute : Attribute,
		ICommandGeneratorAttribute,
		IParameterModifierAttribute,
		ITypeReaderGeneratorAttribute
	{
		public ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
			IServiceProvider services,
			IImmutableCommand original)
			=> new(new[] { new NamedArgumentsCommand(original) });

		public ITypeReader GenerateTypeReader(Type type)
		{
			var readerType = typeof(NamedArgumentsTypeReader<>).MakeGenericType(type);
			return readerType.CreateInstance<ITypeReader>();
		}

		public void ModifyParameter(IParameter parameter)
		{
			var pType = parameter.ParameterType;
			var ppType = typeof(NamedArgumentsParameterPrecondition<>).MakeGenericType(pType);
			parameter.Attributes.Add(Activator.CreateInstance(ppType));
			parameter.MarkAsRemainder();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.NamedArguments
{
	/// <summary>
	/// Specifies how to generate commands/type readers and modify parameters for
	/// named arguments.
	/// </summary>
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = false)]
	public sealed class GenerateNamedArgumentsAttribute : Attribute,
		ICommandGeneratorAttribute,
		IParameterModifierAttribute,
		ITypeReaderGeneratorAttribute
	{
		/// <inheritdoc />
		public ValueTask<IEnumerable<IImmutableCommand>> GenerateCommandsAsync(
			IServiceProvider services,
			IImmutableCommand source)
			=> new(new[] { new NamedArgumentsCommand(source) });

		/// <inheritdoc />
		public ITypeReader GenerateTypeReader(Type type)
		{
			var readerType = typeof(NamedArgumentsTypeReader<>).MakeGenericType(type);
			return readerType.CreateInstance<ITypeReader>();
		}

		/// <inheritdoc />
		public void ModifyParameter(IParameter parameter)
		{
			var pType = parameter.ParameterType;
			var ppType = typeof(NamedArgumentsParameterPrecondition<>).MakeGenericType(pType);
			parameter.Attributes.Add(Activator.CreateInstance(ppType));
			parameter.MarkAsRemainder();
		}
	}
}
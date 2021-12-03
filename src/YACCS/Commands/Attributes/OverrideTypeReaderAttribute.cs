
using YACCS.TypeReaders;

namespace YACCS.Commands.Attributes;

/// <inheritdoc cref="IOverrideTypeReaderAttribute"/>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false)]
public class OverrideTypeReaderAttribute<TReader>
	: Attribute, IOverrideTypeReaderAttribute
	where TReader : ITypeReader, new()
{
	/// <inheritdoc cref="IOverrideTypeReaderAttribute.Reader" />
	public TReader Reader { get; } = new();
	ITypeReader IOverrideTypeReaderAttribute.Reader => Reader;
}

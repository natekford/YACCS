namespace YACCS.TypeReaders;

/// <inheritdoc />
public interface ITypeReaderResult<out T> : ITypeReaderResult
{
	/// <inheritdoc cref="ITypeReaderResult.Value" />
	new T? Value { get; }
}
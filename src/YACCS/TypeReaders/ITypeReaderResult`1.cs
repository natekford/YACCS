namespace YACCS.TypeReaders
{
	public interface ITypeReaderResult<out T> : ITypeReaderResult
	{
		new T? Value { get; }
	}
}
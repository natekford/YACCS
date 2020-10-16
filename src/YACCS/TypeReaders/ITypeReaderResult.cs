using YACCS.Results;

namespace YACCS.TypeReaders
{
	public interface ITypeReaderResult : INestedResult
	{
		object? Value { get; }
	}
}
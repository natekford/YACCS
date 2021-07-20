using System.Diagnostics.CodeAnalysis;

namespace YACCS.TypeReaders
{
	public interface ITypeReaderResult<out T> : ITypeReaderResult
	{
		[MaybeNull]
		new T? Value { get; }
	}
}
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Results
{
	public interface ITypeReaderResult<T> : ITypeReaderResult
	{
		[MaybeNull]
		new T Arg { get; }
	}
}
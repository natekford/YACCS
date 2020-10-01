using System.Diagnostics.CodeAnalysis;

namespace YACCS.Results
{
	public interface ITypeReaderResult<out T> : ITypeReaderResult
	{
		[MaybeNull]
		new T Arg { get; }
	}
}
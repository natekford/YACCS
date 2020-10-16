using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public interface ITypeReader<out T> : ITypeReader
	{
		new ITask<ITypeReaderResult<T>> ReadAsync(IContext context, string input);
	}
}
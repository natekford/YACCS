using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public interface ITypeReaderCache
	{
		ITask<ITypeReaderResult<T>> GetAsync<T>(ITypeReader<T> reader, IContext context, string input, TypeReaderCacheDelegate<T> func);

		void Remove(IContext context);
	}
}
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public interface ITypeReader<T> : ITypeReader
	{
		new Task<ITypeReaderResult<T>> ReadAsync(IContext context, string input);
	}
}
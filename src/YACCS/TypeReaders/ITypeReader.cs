using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public interface ITypeReader
	{
		Task<ITypeReaderResult> ReadAsync(IContext context, string input);
	}
}
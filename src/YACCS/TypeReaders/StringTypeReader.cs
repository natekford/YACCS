using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class StringTypeReader : TypeReader<string>
	{
		public override Task<ITypeReaderResult<string>> ReadAsync(IContext context, string input)
			=> TypeReaderResult<string>.FromSuccess(input).AsTask();
	}
}
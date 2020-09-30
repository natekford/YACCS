using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public interface ITypeReader
	{
		Type OutputType { get; }

		Task<ITypeReaderResult> ReadAsync(IContext context, string input);
	}
}
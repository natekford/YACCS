using System;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public interface ITypeReader
	{
		Type OutputType { get; }

		ITask<ITypeReaderResult> ReadAsync(IContext context, string input);
	}
}
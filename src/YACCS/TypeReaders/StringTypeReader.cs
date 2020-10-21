using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class StringTypeReader : TypeReader<string?>
	{
		public override ITask<ITypeReaderResult<string?>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> TypeReaderResult<string>.FromSuccess(input.Span[0]).AsITask();
	}
}
using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class StringTypeReader : TypeReader<string>
	{
		public override ITask<ITypeReaderResult<string>> ReadAsync(IContext context, string input)
			=> TypeReaderResult<string>.FromSuccess(input).AsITask();
	}
}
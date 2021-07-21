using System;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.TypeReaders
{
	public class StringTypeReader : TypeReader<string?>
	{
		public override ITask<ITypeReaderResult<string?>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var handler = context.Services.GetRequiredService<IArgumentHandler>();

			return TypeReaderResult<string?>.FromSuccess(handler.Join(input)).AsITask();
		}
	}
}
using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.TypeReaders
{
	public class StringTypeReader : TypeReader<string?>
	{
		public override ValueTask<ITypeReaderResult<string?>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var handler = context.Services.GetRequiredService<IArgumentSplitter>();
			return new(TypeReaderResult<string?>.FromSuccess(handler.Join(input)));
		}
	}
}
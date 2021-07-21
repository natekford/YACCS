using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class ArrayTypeReader<T> : TypeReader<T[]>
	{
		private readonly ListTypeReader<T> _ListTypeReader = new();

		public override async ITask<ITypeReaderResult<T[]>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			// It's simpler to have this class rely on an instance of ListTypeReader
			// than to separate out the logic to a base class or static method
			var result = await _ListTypeReader.ReadAsync(context, input).ConfigureAwait(false);
			if (!result.InnerResult.IsSuccess)
			{
				return Error(result.InnerResult);
			}
			return Success(result.Value!.ToArray());
		}
	}
}
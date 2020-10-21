using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class UriTypeReader : TypeReader<Uri>
	{
		public override ITask<ITypeReaderResult<Uri>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var item = input.Span[0];
			if (string.IsNullOrWhiteSpace(item))
			{
				return TypeReaderResult<Uri>.Failure.ITask;
			}

			try
			{
				if (item.StartsWith('<') && item.EndsWith('>'))
				{
					item = item[1..^1];
				}

				return TypeReaderResult<Uri>.FromSuccess(new Uri(item)).AsITask();
			}
			catch
			{
				return TypeReaderResult<Uri>.Failure.ITask;
			}
		}
	}
}
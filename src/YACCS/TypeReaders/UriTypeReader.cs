using System;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class UriTypeReader : TypeReader<Uri>
	{
		public override ITask<ITypeReaderResult<Uri>> ReadAsync(IContext context, string input)
		{
			try
			{
				if (input.StartsWith('<') && input.EndsWith('>'))
				{
					input = input[1..^1];
				}

				return TypeReaderResult<Uri>.FromSuccess(new Uri(input)).AsITask();
			}
			catch
			{
				return TypeReaderResult<Uri>.Failure.ITask;
			}
		}
	}
}
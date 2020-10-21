using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class UriTypeReader : TypeReader<Uri>
	{
		private readonly TypeReaderCacheDelegate<Uri> _CacheDelegate = (_, input) =>
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return TypeReaderResult<Uri>.Failure.ITask;
			}

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
		};

		public override ITask<ITypeReaderResult<Uri>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var cache = context.GetTypeReaderCache();
			return cache.GetAsync(this, context, input.Span[0], _CacheDelegate);
		}
	}
}
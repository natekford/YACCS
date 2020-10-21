using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.TypeReaders
{
	// The interface defines IReadOnlyList<command> while the default implementation is
	// ImmutableArray<command>, but we can't guarantee that the default is what will be used
	// so we should only use IReadOnlyList<command> and the interfaces it implements
	[TypeReaderTargetTypes(
		typeof(IReadOnlyList<IImmutableCommand>),
		typeof(IEnumerable<IImmutableCommand>),
		typeof(IReadOnlyCollection<IImmutableCommand>)
	)]
	public class CommandsTypeReader : TypeReader<IReadOnlyList<IImmutableCommand>>
	{
		private readonly TypeReaderCacheDelegate<IReadOnlyList<IImmutableCommand>> _CacheDelegate = (context, input) =>
		{
			var commands = context.Services.GetRequiredService<ICommandService>();
			var found = commands.Find(input);
			if (found.Count > 0)
			{
				return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.FromSuccess(found).AsITask();
			}
			return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.Failure.ITask;
		};

		public override ITask<ITypeReaderResult<IReadOnlyList<IImmutableCommand>>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var cache = context.GetTypeReaderCache();
			return cache.GetAsync(this, context, input.Span[0], _CacheDelegate);
		}
	}
}
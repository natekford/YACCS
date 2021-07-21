using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// Gets commands which start with the provided value. Order is NOT guaranteed.
	/// </summary>
	[TypeReaderTargetTypes(
		typeof(IEnumerable<IImmutableCommand>),
		typeof(IReadOnlyCollection<IImmutableCommand>)
	)]
	public class CommandsTypeReader : TypeReader<IContext, IReadOnlyCollection<IImmutableCommand>>
	{
		public override ITask<ITypeReaderResult<IReadOnlyCollection<IImmutableCommand>>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var commands = context.Services.GetRequiredService<ICommandService>();

			var found = commands.Find(input);
			if (found.Count > 0)
			{
				return TypeReaderResult<IReadOnlyCollection<IImmutableCommand>>.FromSuccess(found).AsITask();
			}
			return CachedResults<IReadOnlyCollection<IImmutableCommand>>.ParseFailedTask;
		}
	}
}
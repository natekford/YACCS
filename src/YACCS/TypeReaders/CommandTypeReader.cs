using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.TypeReaders
{
	// The interface defines IReadOnlyList<command> while the default implementation is
	// List<command>, but we can't guarantee that the default is what will be used
	// so we should only use IReadOnlyList<command> and the interfaces it implements
	/// <summary>
	/// Gets commands which start with the provided value. Order is NOT guaranteed.
	/// </summary>
	[TypeReaderTargetTypes(
		typeof(IEnumerable<IImmutableCommand>),
		typeof(IReadOnlyCollection<IImmutableCommand>),
		typeof(IReadOnlyList<IImmutableCommand>)
	)]
	public class CommandsTypeReader : TypeReader<IReadOnlyList<IImmutableCommand>>
	{
		public override ITask<ITypeReaderResult<IReadOnlyList<IImmutableCommand>>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var commands = context.Services.GetRequiredService<ICommandService>();

			var found = commands.Find(input.Span[0]);
			if (found.Count > 0)
			{
				return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.FromSuccess(found).AsITask();
			}
			return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.Failure.ITask;
		}
	}
}
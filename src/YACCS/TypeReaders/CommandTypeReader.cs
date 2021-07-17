using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// Gets commands which start with the provided value. Order is NOT guaranteed.
	/// </summary>
	/// <remarks>
	/// The interface defines <see cref="IReadOnlyList{T}"/> while the default implementation is
	/// <see cref="List{T}"/>, but we can't guarantee that the default is what will be used.
	/// So, we should only use <see cref="IReadOnlyList{T}"/> and the interfaces that implements.
	/// </remarks>
	[TypeReaderTargetTypes(
		typeof(IEnumerable<IImmutableCommand>),
		typeof(IReadOnlyCollection<IImmutableCommand>),
		typeof(IReadOnlyList<IImmutableCommand>)
	)]
	public class CommandsTypeReader : TypeReader<IContext, IReadOnlyList<IImmutableCommand>>
	{
		public override ITask<ITypeReaderResult<IReadOnlyList<IImmutableCommand>>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var commands = context.Services.GetRequiredService<ICommandService>();
			var found = commands.Find(input);
			if (found.Count > 0)
			{
				return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.FromSuccess(found).AsITask();
			}
			return CachedResults<IReadOnlyList<IImmutableCommand>>.ParseFailedTask;
		}
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	// The interface defines IReadOnlyList<command> while the default implementation is
	// ImmutableArray<command>, but we can't guarantee that the default is what will be used
	// so we should only use IReadOnlyList<command> and the interfaces it implements
	[TypeReaderTargetTypes(
		//typeof(ImmutableArray<IImmutableCommand>),
		typeof(IReadOnlyList<IImmutableCommand>),
		typeof(IEnumerable<IImmutableCommand>),
		typeof(IReadOnlyCollection<IImmutableCommand>)
	//typeof(IList<IImmutableCommand>),
	//typeof(ICollection<IImmutableCommand>),
	//typeof(IImmutableList<IImmutableCommand>)
	)]
	public class CommandsTypeReader : TypeReader<IReadOnlyList<IImmutableCommand>>
	{
		public override Task<ITypeReaderResult<IReadOnlyList<IImmutableCommand>>> ReadAsync(
			IContext context,
			string input)
		{
			var commands = context.Services.GetRequiredService<ICommandService>();
			var found = commands.Find(input);
			if (found.Count > 0)
			{
				return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.FromSuccess(found).AsTask();
			}
			return TypeReaderResult<IReadOnlyList<IImmutableCommand>>.FailureTask;
		}
	}
}
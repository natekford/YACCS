using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.Trie;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// Parses commands which start with the provided value.
	/// </summary>
	/// <remarks>Order is NOT guaranteed</remarks>
	public class CommandsNameTypeReader : TypeReader<IContext, IReadOnlyCollection<IImmutableCommand>>
	{
		/// <inheritdoc />
		public override ITask<ITypeReaderResult<IReadOnlyCollection<IImmutableCommand>>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var commands = GetCommands(context.Services);

			var node = commands.Commands.Root.FollowPath(input.Span);
			// Generated items have a source and that source gives them the same
			// names/properties, so they should be ignored since they are copies
			var found = node?.GetAllDistinctItems(x => x.Source is null);
			if (found is null || found.Count == 0)
			{
				return CachedResults<IReadOnlyCollection<IImmutableCommand>>.ParseFailed.Task;
			}
			return Success(found).AsITask();
		}

		[GetServiceMethod]
		private static ICommandService GetCommands(IServiceProvider services)
			=> services.GetRequiredService<ICommandService>();
	}
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public class ConsoleCommandService : CommandService
	{
		private readonly ConsoleHandler _Console;

		public ConsoleCommandService(
			ICommandServiceConfig config,
			IArgumentSplitter splitter,
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ConsoleHandler console)
			: base(config, splitter, readers)
		{
			_Console = console;
		}

		protected override Task CommandFinishedAsync(IContext context, IImmutableCommand command)
		{
			_Console.ReleaseIOLocks();
			return base.CommandFinishedAsync(context, command);
		}
	}
}
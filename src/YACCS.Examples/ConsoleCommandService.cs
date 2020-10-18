using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public class ConsoleCommandService : CommandService
	{
		private readonly ConsoleHandler _Console;

		public ConsoleCommandService(
			ICommandServiceConfig config,
			ITypeRegistry<ITypeReader> readers,
			ConsoleHandler console)
			: base(config, readers)
		{
			_Console = console;
		}

		protected override Task CommandFinishedAsync(IContext context, IImmutableCommand command)
		{
			_Console.ReleaseBoth();
			return base.CommandFinishedAsync(context, command);
		}
	}
}
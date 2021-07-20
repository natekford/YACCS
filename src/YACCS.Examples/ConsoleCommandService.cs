using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public sealed class ConsoleCommandService : CommandServiceBase
	{
		private readonly IEnumerable<Assembly> CommandAssemblies;
		private readonly ConsoleHandler Console;
		private readonly Lazy<Task> Initialize;
		private readonly IServiceProvider Services;

		public ConsoleCommandService(
			IServiceProvider services,
			ICommandServiceConfig config,
			IArgumentHandler handler,
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ConsoleHandler console,
			IEnumerable<Assembly> commandAssemblies)
			: base(config, handler, readers)
		{
			CommandAssemblies = commandAssemblies;
			Console = console;
			Services = services;
			Initialize = new Lazy<Task>(() => PrivateInitialize());
		}

		public Task InitializeAsync()
			=> Initialize.Value;

		protected override Task OnCommandExecutedAsync(CommandExecutedEventArgs e)
		{
			Console.WriteResult(e.Result);
			var exceptions = string.Join(Environment.NewLine, e.GetAllExceptions());
			if (!string.IsNullOrWhiteSpace(exceptions))
			{
				Console.WriteLine(exceptions, ConsoleColor.Red);
			}
			return Task.CompletedTask;
		}

		private async Task AddDelegateCommandsAsync()
		{
			// Example delegate command registration
			static int Add(int a, int b)
				=> a + b;

			var @delegate = (Func<int, int, int>)Add;
			var names = new[] { new ImmutableName(new[] { nameof(Add) }) };
			var add = new DelegateCommand(@delegate, names).ToMultipleImmutableAsync(Services);
			await this.AddRangeAsync(add).ConfigureAwait(false);
		}

		private async Task PrivateInitialize()
		{
			foreach (var assembly in CommandAssemblies)
			{
				var commands = assembly.GetAllCommandsAsync(Services);
				await this.AddRangeAsync(commands).ConfigureAwait(false);
			}
			await AddDelegateCommandsAsync().ConfigureAwait(false);
			Debug.WriteLine($"Registered {Commands.Count} commands for '{CultureInfo.CurrentUICulture}'.");
		}
	}
}
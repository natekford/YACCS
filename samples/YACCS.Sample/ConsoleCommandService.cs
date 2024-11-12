using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.Trie;
using YACCS.TypeReaders;

namespace YACCS.Examples;

public sealed class ConsoleCommandService : CommandService
{
	private readonly IEnumerable<Assembly> _CommandAssemblies;
	private readonly Localized<CommandTrie> _Commands;
	private readonly ConsoleHandler _Console;
	private readonly Localized<Lazy<Task>> _Initialize;
	private readonly IServiceProvider _Services;

	public override ITrie<string, IImmutableCommand> Commands
		=> _Commands.GetCurrent();

	public ConsoleCommandService(
		IServiceProvider services,
		CommandServiceConfig config,
		IArgumentHandler handler,
		IReadOnlyDictionary<Type, ITypeReader> readers,
		ConsoleHandler console,
		IEnumerable<Assembly> commandAssemblies)
		: base(config, handler, readers)
	{
		_CommandAssemblies = commandAssemblies;
		_Console = console;
		_Services = services;
		_Initialize = new(_ => new(() => PrivateInitialize()));
		_Commands = new(_ => new CommandTrie(Readers, Config.Separator, Config.CommandNameComparer));
	}

	public Task InitializeAsync()
		=> _Initialize.GetCurrent().Value;

	protected override Task CommandExecutedAsync(CommandExecutedEventArgs e)
	{
		_Console.WriteResult(e.Result);
		var exceptions = string.Join(Environment.NewLine, e.GetAllExceptions());
		if (!string.IsNullOrWhiteSpace(exceptions))
		{
			_Console.WriteLine(exceptions, ConsoleColor.Red);
		}
		return base.CommandExecutedAsync(e);
	}

	private async Task AddDelegateCommandsAsync()
	{
		// Example delegate command registration
		static int Add(int a, int b)
			=> a + b;

		var paths = new[] { LocalizedPath.New(nameof(Add)) };
		var add = new DelegateCommand(Add, paths);
		await foreach (var command in add.ToMultipleImmutableAsync(_Services))
		{
			Commands.Add(command);
		}
	}

	private async Task PrivateInitialize()
	{
		foreach (var assembly in _CommandAssemblies)
		{
			// I don't use the returned type for anything, but if you want to load/unload
			// "modules" that's why the defining type is returned along with the command
			await foreach (var (_, command) in assembly.GetAllCommandsAsync(_Services))
			{
				Commands.Add(command);
			}
		}
		await AddDelegateCommandsAsync().ConfigureAwait(false);
		Debug.WriteLine($"Registered {Commands.Count} commands for '{CultureInfo.CurrentUICulture}'.");
	}
}
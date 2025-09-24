﻿using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using YACCS.Commands;
using YACCS.Help;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.Sample.Interactivity;
using YACCS.TypeReaders;

namespace YACCS.Sample;

public sealed class Program
{
	private readonly IServiceProvider _Services;

	private Program()
	{
		_Services = new ServiceCollection()
			.AddSingleton(CommandServiceConfig.Default)
			.AddSingleton<ILocalizer>(Localize.Instance)
			.AddSingleton<ConsoleCommandService>()
			.AddSingleton<ConsoleHandler>()
			.AddSingleton<ConsoleInput>()
			.AddSingleton<ConsolePaginator>()
			.AddSingleton<IEnumerable<Assembly>>([typeof(Program).Assembly])
			.AddSingleton<ICommandService>(x => x.GetRequiredService<ConsoleCommandService>())
			.AddSingleton<IArgumentHandler>(x =>
			{
				var config = x.GetRequiredService<CommandServiceConfig>();
				return new ArgumentHandler(
					config.Separator,
					config.StartQuotes,
					config.EndQuotes
				);
			})
			.AddSingleton<IFormatProvider, ConsoleTagFormatter>()
			.AddSingleton<StringHelpFactory>()
			.AddSingleton<IReadOnlyDictionary<Type, ITypeReader>, TypeReaderRegistry>()
			.AddSingleton<IReadOnlyDictionary<Type, string>, TypeNameRegistry>()
			.BuildServiceProvider();

		_Services.ThrowIfUnregisteredServices();

#if DEBUG
		Localize.Instance.KeyNotFound += (key, culture)
			=> System.Diagnostics.Debug.WriteLine($"Unable to find the localization for '{key}' in '{culture}'.");
#endif
	}

	public static Task Main()
		=> new Program().RunAsync();

	private async Task RunAsync()
	{
		while (true)
		{
			var console = _Services.GetRequiredService<ConsoleHandler>();
			// This has to be inside the while loop to handle creating localized commands
			var commandService = _Services.GetRequiredService<ConsoleCommandService>();
			await commandService.InitializeAsync().ConfigureAwait(false);

			// Wait until the locks are released so we don't print out the
			// prompt before the output from the command is printed
			await console.WaitForIOLockAsync().ConfigureAwait(false);
			console.ReleaseIOLock();
			console.WriteLine("Enter a command and its arguments: ");

			var input = await console.ReadLineAsync().ConfigureAwait(false);
			if (string.IsNullOrWhiteSpace(input))
			{
				continue;
			}

			// Locks get released when context is disposed by CommandFinishedAsync
			await console.WaitForIOLockAsync().ConfigureAwait(false);
			var context = new ConsoleContext(_Services.CreateScope(), input);
			await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
		}
	}
}
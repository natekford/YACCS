using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using YACCS.Commands;
using YACCS.Examples.Interactivity;
using YACCS.Help;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Examples;

public sealed class Program
{
	private readonly IServiceProvider _Services;

	private Program()
	{
		_Services = new ServiceCollection()
			.AddSingleton(CommandServiceConfig.Instance)
			.AddSingleton<ILocalizer>(Localize.Instance)
			.AddSingleton<ConsoleCommandService>()
			.AddSingleton<ConsoleHandler>()
			.AddSingleton<ConsoleInput>()
			.AddSingleton<ConsoleInteractivityManager>()
			.AddSingleton<ConsolePaginator>()
			.AddSingleton<IEnumerable<Assembly>>(new[] { typeof(Program).Assembly })
			.AddSingleton<ICommandService>(x => x.GetRequiredService<ConsoleCommandService>())
			.AddSingleton<IArgumentHandler>(x =>
			{
				var config = x.GetRequiredService<ICommandServiceConfig>();
				return new ArgumentHandler(
					config.Separator,
					config.StartQuotes,
					config.EndQuotes
				);
			})
			.AddSingleton<IFormatProvider, ConsoleTagFormatter>()
			.AddSingleton<IHelpFormatter, HelpFormatter>()
			.AddSingleton<IReadOnlyDictionary<Type, ITypeReader>, TypeReaderRegistry>()
			.AddSingleton<IReadOnlyDictionary<Type, string>, TypeNameRegistry>()
			.BuildServiceProvider();

		_Services
			.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>()
			.ThrowIfUnregisteredServices(_Services);

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
			var commands = _Services.GetRequiredService<ConsoleCommandService>();
			await commands.InitializeAsync().ConfigureAwait(false);

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
			var result = await commands.ExecuteAsync(context, input).ConfigureAwait(false);
			// If a command cannot be executed, context must be disposed here
			if (!result.InnerResult.IsSuccess)
			{
				context.Dispose();
			}
			console.WriteResult(result);
		}
	}
}
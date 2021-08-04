using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Examples.Interactivity;
using YACCS.Help;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public sealed class Program
	{
		private readonly IServiceProvider _Services;

		private Program()
		{
			_Services = new ServiceCollection()
				.AddSingleton<ICommandServiceConfig>(CommandServiceConfig.Instance)
				.AddSingleton<ILocalizer>(Localize.Instance)
				.AddSingleton<ConsoleCommandService>()
				.AddSingleton<ConsoleHandler>()
				.AddSingleton<ConsoleInput>()
				.AddSingleton<ConsoleInteractivityManager>()
				.AddSingleton<ConsolePaginator>()
				.AddSingleton<IEnumerable<Assembly>>(new[] { typeof(Program).Assembly })
				.AddSingleton<ICommandService>(x => x.GetRequiredService<ConsoleCommandService>())
				.AddSingleton<IArgumentHandler, ArgumentHandler>()
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
				var commands = _Services.GetRequiredService<ConsoleCommandService>();
				await commands.InitializeAsync().ConfigureAwait(false);

				await console.WaitForBothIOLocksAsync().ConfigureAwait(false);
				console.ReleaseIOLocks();
				console.WriteLine("Enter a command and its arguments: ");

				var input = await console.ReadLineAsync().ConfigureAwait(false);
				if (string.IsNullOrWhiteSpace(input))
				{
					continue;
				}

				var context = new ConsoleContext(_Services.CreateScope(), input);
				var result = await commands.ExecuteAsync(context, input).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess)
				{
					console.ReleaseIOLocks();
				}
				console.WriteResult(result);
				console.WriteLine();
			}
		}
	}
}
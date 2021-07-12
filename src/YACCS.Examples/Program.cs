using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Help;
using YACCS.Interactivity.Input;
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
				.AddSingleton<IArgumentSplitter>(ArgumentSplitter.Instance)
				.AddSingleton<ICommandServiceConfig>(CommandServiceConfig.Instance)
				.AddSingleton<ILocalizer>(Localize.Instance)
				.AddSingleton<ConsoleHandler>()
				.AddSingleton<ConsoleCommandServiceFactory>()
				.AddSingleton<IEnumerable<Assembly>>(new[] { typeof(Program).Assembly })
				// These 2 need to be transient so if the culture is changed
				// the correct localized command service is retrieved
				.AddTransient<ICommandService>(x => x.GetRequiredService<ConsoleCommandService>())
				.AddTransient<ConsoleCommandService>(x =>
				{
					return x.GetRequiredService<ConsoleCommandServiceFactory>()
						.GetCommandService();
				})
				.AddSingleton<IFormatProvider, TagFormatter>()
				.AddSingleton<IHelpFormatter, HelpFormatter>()
				.AddSingleton<IInput<IContext, string>, ConsoleInput>()
				.AddSingleton<IReadOnlyDictionary<Type, ITypeReader>, TypeReaderRegistry>()
				.AddSingleton<IReadOnlyDictionary<Type, string>, TypeNameRegistry>()
				.BuildServiceProvider();

#if DEBUG
			Localize.Instance.KeyNotFound += (key, culture)
				=> Debug.WriteLine($"Unable to find the localization for '{key}' in '{culture}'.");
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
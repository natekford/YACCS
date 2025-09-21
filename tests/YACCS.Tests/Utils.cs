using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Help;
using YACCS.Parsing;
using YACCS.TypeReaders;

using static YACCS.Commands.CommandCreationUtils;

namespace YACCS.Tests;

public static class Utils
{
	public static async Task AddRangeAsync(
		this CommandService commandService,
		IAsyncEnumerable<ImmutableReflectionCommand> enumerable)
	{
		await foreach (var (_, command) in enumerable)
		{
			commandService.Commands.Add(command);
		}
	}

	public static IServiceCollection CreateServiceCollection(
		CommandServiceConfig? config = null)
	{
		config ??= CommandServiceConfig.Default;
		var handler = new ArgumentHandler(
			config.Separator,
			config.StartQuotes,
			config.EndQuotes
		);
		var readers = new TypeReaderRegistry();
		var commandService = new FakeCommandService(config, handler, readers);
		var names = new TypeNameRegistry();
		var formatter = new TagFormatter();
		var helpFormatter = new HelpFormatter(names, formatter);

		return new ServiceCollection()
			.AddSingleton<IArgumentHandler>(handler)
			.AddSingleton(config)
			.AddSingleton<IReadOnlyDictionary<Type, ITypeReader>>(readers)
			.AddSingleton(readers)
			.AddSingleton<IReadOnlyDictionary<Type, string>>(names)
			.AddSingleton(names)
			.AddSingleton<IFormatProvider>(formatter)
			.AddSingleton(formatter)
			.AddSingleton<IHelpFormatter>(helpFormatter)
			.AddSingleton(helpFormatter)
			.AddSingleton<ICommandService>(commandService)
			.AddSingleton(commandService);
	}

	public static IServiceProvider CreateServices(CommandServiceConfig? config = null)
		=> CreateServiceCollection(config).BuildServiceProvider();

	public static T Get<T>(this IServiceProvider services) where T : notnull
		=> ServiceProviderServiceExtensions.GetRequiredService<T>(services);

	public static T Get<T>(this IContext context) where T : notnull
		=> context.Services.Get<T>();
}
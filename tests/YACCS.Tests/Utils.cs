using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Parsing;
using YACCS.TypeReaders;

using static YACCS.Commands.CommandCreationUtils;

namespace YACCS.Tests
{
	public static class Utils
	{
		public static async Task AddRangeAsync(
			this CommandServiceBase commandService,
			IAsyncEnumerable<ReflectedCommand> enumerable)
		{
			await foreach (var (_, command) in enumerable)
			{
				commandService.Commands.Add(command);
			}
		}

		public static IServiceCollection CreateServiceCollection(ICommandServiceConfig? config = null)
		{
			config ??= CommandServiceConfig.Instance;
			var handler = new ArgumentHandler(config);
			var readers = new TypeReaderRegistry();
			var commandService = new CommandService(config, handler, readers);

			return new ServiceCollection()
				.AddSingleton<IArgumentHandler>(handler)
				.AddSingleton<ICommandServiceConfig>(config)
				.AddSingleton<IReadOnlyDictionary<Type, ITypeReader>>(readers)
				.AddSingleton<TypeReaderRegistry>(readers)
				.AddSingleton<ICommandService>(commandService)
				.AddSingleton<CommandService>(commandService);
		}

		public static IServiceProvider CreateServices(ICommandServiceConfig? config = null)
			=> CreateServiceCollection(config).BuildServiceProvider();

		public static T Get<T>(this IServiceProvider services) where T : notnull
			=> ServiceProviderServiceExtensions.GetRequiredService<T>(services);

		public static T Get<T>(this IContext context) where T : notnull
			=> context.Services.Get<T>();
	}
}
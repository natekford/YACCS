using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Tests
{
	public static class Utils
	{
		public static IServiceCollection CreateServiceCollection(ICommandServiceConfig? config = null)
		{
			config ??= CommandServiceConfig.Instance;
			var splitter = new ArgumentSplitter(config);
			var readers = new TypeReaderRegistry();
			var commandService = new CommandService(config, splitter, readers);

			return new ServiceCollection()
				.AddSingleton<IArgumentSplitter>(splitter)
				.AddSingleton<ICommandServiceConfig>(config)
				.AddSingleton<IReadOnlyDictionary<Type, ITypeReader>>(readers)
				.AddSingleton<TypeReaderRegistry>(readers)
				.AddSingleton<ICommandService>(commandService)
				.AddSingleton<CommandService>(commandService);
		}

		public static IServiceProvider CreateServices(ICommandServiceConfig? config = null)
			=> CreateServiceCollection(config).BuildServiceProvider();

		public static T Get<T>(this IServiceProvider services) where T : notnull
			=> services.GetRequiredService<T>();

		public static T Get<T>(this IContext context) where T : notnull
			=> context.Services.Get<T>();
	}
}
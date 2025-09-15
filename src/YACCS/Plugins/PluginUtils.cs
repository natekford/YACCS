using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands;

using static YACCS.Commands.CommandCreationUtils;

namespace YACCS.Plugins;

/// <summary>
/// Utilities for plugins.
/// </summary>
public static class PluginUtils
{
	/// <summary>
	/// For each assembly in <paramref name="assemblies"/>, gets each supported culture
	/// via <see cref="PluginAttribute"/> and then creates commands after
	/// setting <see cref="CultureInfo.CurrentUICulture"/> to each subsequent culture.
	/// </summary>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <param name="assemblies">The assemblies to look through.</param>
	/// <returns>A dictionary of cultures and reflected commands.</returns>
	public static async Task<Dictionary<CultureInfo, List<ImmutableReflectionCommand>>> CreateCommandsInSupportedCultures(
		this IServiceProvider services,
		IEnumerable<Assembly> assemblies)
	{
		var dict = new Dictionary<CultureInfo, List<ImmutableReflectionCommand>>();
		var originalCulture = CultureInfo.CurrentUICulture;
		foreach (var assembly in assemblies)
		{
			var attr = assembly.GetCustomAttribute<PluginAttribute>();
			if (attr is null)
			{
				continue;
			}

			foreach (var culture in attr.SupportedCultures.Select(CultureInfo.GetCultureInfo))
			{
				// In case any commands have to be initialized in the culture they're
				// going to be used in
				CultureInfo.CurrentUICulture = culture;

				await foreach (var command in assembly.GetAllCommandsAsync(services))
				{
					if (!dict.TryGetValue(culture, out var list))
					{
						dict.Add(culture, list = []);
					}
					list.Add(command);
				}
			}
		}
		CultureInfo.CurrentUICulture = originalCulture;
		return dict;
	}

	/// <summary>
	/// Gets all instantiators from <paramref name="pluginAssemblies"/>,
	/// adds services to <paramref name="serviceCollection"/>,
	/// and then configures the added services.
	/// </summary>
	/// <param name="serviceCollection">
	/// The services to add to, and later create a <see cref="IServiceProvider"/> from.
	/// </param>
	/// <param name="pluginAssemblies">The assemblies to treat as plugins.</param>
	/// <param name="createServiceProvider">
	/// Creates a <see cref="IServiceProvider"/> from <paramref name="serviceCollection"/>.
	/// </param>
	/// <returns></returns>
	public static async Task<IServiceProvider> InstantiatePlugins<TServiceCollection>(
		this TServiceCollection serviceCollection,
		IEnumerable<Assembly> pluginAssemblies,
		Func<TServiceCollection, IServiceProvider> createServiceProvider)
	{
		var plugins = pluginAssemblies
			.Select(x => x.GetCustomAttribute<PluginAttribute>())
			.Where(x => x is not null)
			.ToList();

		foreach (var plugin in plugins)
		{
			await plugin.AddServicesAsync(serviceCollection!).ConfigureAwait(false);
		}

		var serviceProvider = createServiceProvider(serviceCollection);
		foreach (var plugin in plugins)
		{
			await plugin.ConfigureServicesAsync(serviceProvider).ConfigureAwait(false);
		}

		return serviceProvider;
	}

	/// <summary>
	/// Loads each assembly and checks if it's marked with <see cref="PluginAttribute"/>.
	/// </summary>
	/// <param name="dllPaths">The dlls to load as plugin assemblies.</param>
	/// <returns>A dictionary of plugin assemblies.</returns>
	/// <remarks>All assemblies passed into this method will be loaded.</remarks>
	public static Dictionary<string, Assembly> LoadPluginAssemblies(IEnumerable<string> dllPaths)
	{
		return dllPaths
			.Select(x =>
			{
				try
				{
					return Assembly.LoadFrom(x);
				}
				catch (BadImageFormatException)
				{
					return null;
				}
			})
			.Where(x => x?.GetCustomAttribute<PluginAttribute>() is not null)
			.ToDictionary(x => x!.FullName, x => x!);
	}
}
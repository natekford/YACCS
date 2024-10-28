using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Plugins;

using static YACCS.Commands.CommandCreationUtils;

namespace YACCS.CommandAssemblies;

/// <summary>
/// Utilities for plugins.
/// </summary>
public static class PluginUtils
{
	/// <summary>
	/// For each assembly in <paramref name="assemblies"/>, gets each supported culture
	/// via <see cref="SupportedCulturesAttribute"/> and then creates commands after
	/// setting <see cref="CultureInfo.CurrentUICulture"/> to each subsequent culture.
	/// </summary>
	/// <param name="assemblies">The assemblies to look through.</param>
	/// <param name="services">The services to use for dependency injection.</param>
	/// <returns>A dictionary of cultures and reflected commands.</returns>
	public static async Task<Dictionary<CultureInfo, List<ImmutableReflectionCommand>>> GetCommandsInSupportedCultures(
		this IEnumerable<Assembly> assemblies,
		IServiceProvider services)
	{
		var dict = new Dictionary<CultureInfo, List<ImmutableReflectionCommand>>();
		var originalCulture = CultureInfo.CurrentUICulture;
		foreach (var assembly in assemblies)
		{
			var attr = assembly.GetCustomAttribute<SupportedCulturesAttribute>();
			if (attr is null)
			{
				continue;
			}

			foreach (var culture in attr.SupportedCultures)
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
	/// Gets all instantiators from attributes on an assembly that
	/// implement <see cref="PluginAttribute{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of service collection that is being used.</typeparam>
	/// <param name="assemblies">The assemblies to look through.</param>
	/// <returns>A list of service instantiators.</returns>
	public static List<PluginAttribute<T>> GetPlugins<T>(this IEnumerable<Assembly> assemblies)
	{
		return assemblies
			.Select(x => x.CustomAttributes.OfType<PluginAttribute<T>>().SingleOrDefault())
			.Where(x => x is not null)
			.ToList();
	}

	/// <summary>
	/// Loads each assembly and checks if it's marked with
	/// <see cref="PluginAttribute"/>.
	/// </summary>
	/// <param name="dlls">The dlls to load as plugin assemblies.</param>
	/// <returns>A dictionary of plugin assemblies.</returns>
	public static Dictionary<string, Assembly> Load(this IEnumerable<string> dlls)
	{
		return dlls
			.Select(x => Assembly.LoadFrom(x))
			.Where(x => x.GetCustomAttribute<PluginAttribute>() is not null)
			.ToDictionary(x => x.FullName, x => x);
	}
}
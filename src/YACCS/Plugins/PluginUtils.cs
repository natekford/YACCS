using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands;

using static YACCS.Commands.CommandCreationUtils;

namespace YACCS.CommandAssemblies
{
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
		public static async Task<ConcurrentDictionary<CultureInfo, List<ReflectedCommand>>> GetCommandsInSupportedCultures(
			this IEnumerable<Assembly> assemblies,
			IServiceProvider services)
		{
			var dict = new ConcurrentDictionary<CultureInfo, List<ReflectedCommand>>();
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
						dict.GetOrAdd(culture, _ => new()).Add(command);
					}
				}
			}
			CultureInfo.CurrentUICulture = originalCulture;
			return dict;
		}

		/// <summary>
		/// Gets all instantiators defined via <see cref="ServiceInstantiatorAttribute"/> and
		/// implement <see cref="IServiceInstantiator{T}"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assemblies">The assemblies to look through.</param>
		/// <returns>A list of service instantiators.</returns>
		public static List<IServiceInstantiator<T>> GetInstantiators<T>(
			this IEnumerable<Assembly> assemblies)
		{
			var instantiators = new List<IServiceInstantiator<T>>();
			foreach (var assembly in assemblies)
			{
				var attribute = assembly.CustomAttributes
					.OfType<ServiceInstantiatorAttribute>()
					.SingleOrDefault();
				if (attribute?.Instantiator is IServiceInstantiator<T> instantiator)
				{
					instantiators.Add(instantiator);
				}
			}
			return instantiators;
		}

		/// <summary>
		/// Loads each assembly and checks if it's marked with <see cref="PluginAttribute"/>.
		/// </summary>
		/// <param name="dlls">The dlls to check if they are plugins.</param>
		/// <returns>A dictionary of plugin assemblies.</returns>
		public static Dictionary<string, Assembly> Load(IEnumerable<string> dlls)
		{
			var dictionary = new Dictionary<string, Assembly>();
			foreach (var file in dlls)
			{
				var assembly = Assembly.LoadFrom(file);
				if (assembly.GetCustomAttribute<PluginAttribute>() is not null)
				{
					dictionary.Add(assembly.FullName, assembly);
				}
			}
			return dictionary;
		}

		/// <summary>
		/// Calls <see cref="Load(IEnumerable{string})"/> with all dlls
		/// in <paramref name="directory"/>.
		/// </summary>
		/// <param name="directory">The directory to gather dlls from</param>
		/// <returns>A dictionary of plugin assemblies.</returns>
		public static Dictionary<string, Assembly> Load(string directory)
			=> Load(Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly));

		/// <summary>
		/// Throws an exception if there are no assemblies in <paramref name="assemblies"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assemblies">The dictionary of assemblies.</param>
		/// <returns>The passed in value after it has been checked.</returns>
		public static T ThrowIfEmpty<T>(this T assemblies) where T : IDictionary<string, Assembly>
		{
			if (assemblies.Count == 0)
			{
				throw new DllNotFoundException("Unable to find any command assemblies.");
			}
			return assemblies;
		}
	}
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands;

using static YACCS.Commands.CommandServiceUtils;

namespace YACCS.CommandAssemblies
{
	public static class PluginUtils
	{
		public static void Add(this IDictionary<string, Assembly> dictionary, Assembly assembly)
			=> dictionary.Add(assembly.FullName, assembly);

		public static async Task AddServicesAsync<T>(
			this IEnumerable<IServiceInstantiator<T>> instantiators,
			T services)
		{
			foreach (var instantiator in instantiators)
			{
				await instantiator.AddServicesAsync(services).ConfigureAwait(false);
			}
		}

		public static async Task ConfigureServicesAsync(
			this IEnumerable<IServiceInstantiator> instantiators,
			IServiceProvider services)
		{
			foreach (var instantiator in instantiators)
			{
				await instantiator.ConfigureServicesAsync(services).ConfigureAwait(false);
			}
		}

		public static async Task<IDictionary<CultureInfo, List<CreatedCommand>>> GetCommandsInSupportedCultures(
			this IEnumerable<Assembly> assemblies,
			IServiceProvider services)
		{
			var dict = new ConcurrentDictionary<CultureInfo, List<CreatedCommand>>();
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

		public static async Task Instantiate<T>(
			this IEnumerable<IServiceInstantiator<T>> instantiators,
			T services,
			Func<T, IServiceProvider> providerFactory)
		{
			await instantiators.AddServicesAsync(services).ConfigureAwait(false);
			var provider = providerFactory.Invoke(services);
			await instantiators.ConfigureServicesAsync(provider).ConfigureAwait(false);
		}

		public static Dictionary<string, Assembly> Load(IEnumerable<string> files)
		{
			var dictionary = new Dictionary<string, Assembly>();
			foreach (var file in files)
			{
				var assembly = Assembly.LoadFrom(file);
				if (assembly.GetCustomAttribute<PluginAttribute>() is not null)
				{
					dictionary.Add(assembly);
				}
			}
			return dictionary;
		}

		public static Dictionary<string, Assembly> Load(string directory)
			=> Load(Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly));

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
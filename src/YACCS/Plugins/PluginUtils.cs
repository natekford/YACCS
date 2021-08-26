using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace YACCS.CommandAssemblies
{
	public static class PluginUtils
	{
		public static void Add(this IDictionary<string, Assembly> dictionary, Assembly assembly)
			=> dictionary.Add(assembly.FullName, assembly);

		public static async Task AddServicesAsync<T>(
			this IEnumerable<IServiceInstantiator> instantiators,
			T services,
			bool throwIfWrongType = false)
		{
			foreach (var instantiator in instantiators)
			{
				if (instantiator is IServiceInstantiator<T> typed)
				{
					await typed.AddServicesAsync(services).ConfigureAwait(false);
				}
				else if (throwIfWrongType)
				{
					await instantiator.AddServicesAsync(services!).ConfigureAwait(false);
				}
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

		public static List<IServiceInstantiator> GetInstantiators(
			this IEnumerable<Assembly> assemblies)
		{
			var instantiators = new List<IServiceInstantiator>();
			foreach (var assembly in assemblies)
			{
				var attribute = assembly.CustomAttributes
					.OfType<ServiceInstantiatorAttribute>()
					.SingleOrDefault();
				if (attribute?.Instantiator is IServiceInstantiator instantiator)
				{
					instantiators.Add(instantiator);
				}
			}
			return instantiators;
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
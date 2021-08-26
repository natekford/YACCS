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

		public static async Task Configure(
			this IEnumerable<IServiceInstantiator> instantiators,
			IServiceProvider services)
		{
			foreach (var instantiator in instantiators)
			{
				await instantiator.AddServicesAsync(services).ConfigureAwait(false);
			}
		}

		public static Dictionary<string, Assembly> Find(string directory)
		{
			var dictionary = new Dictionary<string, Assembly>();
			foreach (var file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
			{
				var assembly = Assembly.LoadFrom(file);
				if (assembly.GetCustomAttribute<PluginAttribute>() is not null)
				{
					dictionary.Add(assembly);
				}
			}
			if (dictionary.Count > 0)
			{
				return dictionary;
			}
			throw new DllNotFoundException("Unable to find any command assemblies.");
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

		public static async Task Instantiate<T>(
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
	}
}
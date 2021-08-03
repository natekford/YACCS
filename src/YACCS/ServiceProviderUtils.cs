using System;
using System.Diagnostics.CodeAnalysis;

namespace YACCS
{
	// These are only here to prevent adding a service provider implementation dependency.
	internal static class ServiceProviderUtils
	{
		internal static T GetRequiredService<T>(this IServiceProvider provider)
		{
			var service = provider.GetService(typeof(T));
			if (service is T t)
			{
				return t;
			}
			throw new InvalidOperationException(
				$"{typeof(T).Name} does not have a registered service.");
		}

		[return: MaybeNull]
		internal static T GetService<T>(this IServiceProvider provider)
			=> provider.GetService(typeof(T)) is T t ? t : default;
	}
}
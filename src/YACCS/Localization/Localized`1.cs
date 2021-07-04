using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace YACCS.Localization
{
	public static class Localized
	{
		public static Localized<T> Create<T>() where T : new()
			=> new(_ => new());
	}

	public sealed class Localized<T>
	{
		private readonly ConcurrentDictionary<CultureInfo, T> _Source = new();
		private readonly Func<CultureInfo, T> _ValueFactory;

		public Localized(Func<CultureInfo, T> valueFactory)
		{
			_ValueFactory = valueFactory;
		}

		public T Get()
			=> _Source.GetOrAdd(CultureInfo.CurrentUICulture, _ValueFactory);
	}
}
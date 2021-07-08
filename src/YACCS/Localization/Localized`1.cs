using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace YACCS.Localization
{
	public static class Localized
	{
		public static Localized<T> Create<T>() where T : new()
			=> new(_ => new());
	}

	public sealed class Localized<T> : IReadOnlyDictionary<CultureInfo, T>
	{
		private readonly ConcurrentDictionary<CultureInfo, T> _Dict = new();
		private readonly Func<CultureInfo, T> _ValueFactory;

		public int Count => _Dict.Count;
		public IEnumerable<CultureInfo> Keys => _Dict.Keys;
		public IEnumerable<T> Values => _Dict.Values;

		public T this[CultureInfo? key]
			=> _Dict.GetOrAdd(key ?? CultureInfo.CurrentUICulture, _ValueFactory);

		public Localized(Func<CultureInfo, T> valueFactory)
		{
			_ValueFactory = valueFactory;
		}

		public bool ContainsKey(CultureInfo key)
			=> _Dict.ContainsKey(key);

		public IEnumerator<KeyValuePair<CultureInfo, T>> GetEnumerator()
			=> _Dict.GetEnumerator();

		public bool TryGetValue(CultureInfo key, out T value)
			=> _Dict.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
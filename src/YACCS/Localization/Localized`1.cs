using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;

namespace YACCS.Localization;

/// <summary>
/// Utilities for <see cref="Localized{T}"/>.
/// </summary>
public static class Localized
{
	/// <inheritdoc cref="Localized{T}(Func{CultureInfo, T})"/>
	public static Localized<T> Create<T>() where T : new()
		=> new(_ => new());
}

/// <summary>
/// Creates an instance of a specified value for each culture.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Localized<T> : IReadOnlyDictionary<CultureInfo, T>
{
	private readonly ConcurrentDictionary<CultureInfo, T> _Dict = new();
	private readonly Func<CultureInfo, T> _ValueFactory;

	/// <inheritdoc />
	public int Count => _Dict.Count;
	/// <inheritdoc />
	public IEnumerable<CultureInfo> Keys => _Dict.Keys;
	/// <inheritdoc />
	public IEnumerable<T> Values => _Dict.Values;

	/// <inheritdoc />
	public T this[CultureInfo key]
		=> _Dict.GetOrAdd(EnsureKey(key), _ValueFactory);

	/// <summary>
	/// Creates a new <see cref="Localized{T}"/>.
	/// </summary>
	/// <param name="valueFactory"></param>
	public Localized(Func<CultureInfo, T> valueFactory)
	{
		_ValueFactory = valueFactory;
	}

	/// <inheritdoc />
	public bool ContainsKey(CultureInfo? key)
		=> _Dict.ContainsKey(EnsureKey(key));

	/// <summary>
	/// Get the value for <see cref="CultureInfo.CurrentUICulture"/>.
	/// </summary>
	/// <inheritdoc cref="this[CultureInfo]" />
	public T GetCurrent()
		=> this[EnsureKey(null)];

	/// <inheritdoc />
	public IEnumerator<KeyValuePair<CultureInfo, T>> GetEnumerator()
		=> _Dict.GetEnumerator();

	/// <inheritdoc />
	public bool TryGetValue(CultureInfo? key, out T value)
		=> _Dict.TryGetValue(EnsureKey(key), out value);

	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

	private CultureInfo EnsureKey(CultureInfo? key)
		=> key ?? CultureInfo.CurrentUICulture;
}

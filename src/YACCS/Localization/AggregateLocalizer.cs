using System.Collections.Immutable;
using System.Globalization;

namespace YACCS.Localization;

/// <summary>
/// Combines multiple <see cref="ILocalizer"/> into one.
/// </summary>
public class AggregateLocalizer : ILocalizer
{
	private ImmutableArray<ILocalizer> _Localizers = [];

	/// <summary>
	/// Whether or not there are any localizers inside this one.
	/// </summary>
	public bool IsEmpty => _Localizers.Length == 0;

	/// <summary>
	/// Fires when a key isn't found in any of the localizers.
	/// </summary>
	public event Action<string, CultureInfo>? KeyNotFound;

	/// <summary>
	/// Adds <paramref name="localizer"/> to the end of the list.
	/// </summary>
	/// <param name="localizer">The localizer to add.</param>
	public void Append(ILocalizer localizer)
	{
		ImmutableInterlocked.Update(ref _Localizers, static (array, arg) =>
		{
			return array.Add(arg);
		}, localizer);
	}

	/// <inheritdoc />
	public string? Get(string key, CultureInfo? culture = null)
	{
		culture ??= CultureInfo.CurrentUICulture;

		foreach (var localizer in _Localizers)
		{
			if (localizer.Get(key, culture) is string s)
			{
				return s;
			}
		}

		KeyNotFound?.Invoke(key, culture);
		return null;
	}

	/// <summary>
	/// Adds <paramref name="localizer"/> to the beginning of the list.
	/// </summary>
	/// <param name="localizer">The localizer to add.</param>
	public void Prepend(ILocalizer localizer)
	{
		ImmutableInterlocked.Update(ref _Localizers, static (array, arg) =>
		{
			return array.Insert(0, arg);
		}, localizer);
	}
}
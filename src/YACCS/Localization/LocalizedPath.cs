using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands;

namespace YACCS.Localization;

/// <summary>
/// An immutable list of strings which supports localization
/// through <see cref="Localize.This(string, string?)"/>.
/// </summary>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public sealed class LocalizedPath : IReadOnlyList<string>
{
	/// <inheritdoc />
	public int Count => Keys.Length;
	/// <summary>
	/// The unlocalized keys used for this path.
	/// </summary>
	public ImmutableArray<string> Keys { get; }
	private string DebuggerDisplay => $"Name = {ToString()}, Count = {Keys.Length}";

	/// <inheritdoc />
	public string this[int index] => Localize.This(Keys[index]);

	/// <summary>
	/// Creates a new <see cref="LocalizedPath"/>.
	/// </summary>
	/// <param name="keys">The values to use as parts of the name.</param>
	public LocalizedPath(IEnumerable<string> keys)
	{
		if (keys is LocalizedPath name)
		{
			Keys = name.Keys;
		}
		else
		{
			Keys = [.. keys];
		}
	}

	/// <summary>
	/// Creates a new <see cref="LocalizedPath"/>.
	/// </summary>
	/// <param name="keys">The values to use as parts of the name.</param>
	/// <returns>An immutable path.</returns>
	public static LocalizedPath New(params string[] keys)
		=> new(keys);

	/// <inheritdoc />
	public IEnumerator<string> GetEnumerator()
	{
		foreach (var key in Keys)
		{
			yield return Localize.This(key);
		}
	}

	/// <inheritdoc />
	public override string ToString()
		=> string.Join(CommandServiceUtils.SPACE, this);

	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();
}
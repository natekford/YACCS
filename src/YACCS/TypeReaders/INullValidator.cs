using System;

namespace YACCS.TypeReaders;

/// <summary>
/// Defines a method for determining if input represents null.
/// </summary>
public interface INullValidator
{
	/// <summary>
	/// Whether the passed in input represents <see langword="null"/>.
	/// </summary>
	/// <param name="input">The input to check.</param>
	/// <returns>A bool indicating if <paramref name="input"/> represents null.</returns>
	bool IsNull(ReadOnlyMemory<string?> input);
}
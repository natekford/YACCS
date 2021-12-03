
using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a value from strings.
/// </summary>
public interface ITypeReader
{
	/// <summary>
	/// The context type for <see cref="ReadAsync(IContext, ReadOnlyMemory{string})"/>.
	/// </summary>
	Type ContextType { get; }
	/// <summary>
	/// The output type from <see cref="ReadAsync(IContext, ReadOnlyMemory{string})"/>.
	/// </summary>
	Type OutputType { get; }

	/// <summary>
	/// Parses <paramref name="input"/> into a value.
	/// </summary>
	/// <param name="context">
	/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
	/// </param>
	/// <param name="input">The input to parse.</param>
	/// <returns>A type reader result indicating success or failure.</returns>
	ITask<ITypeReaderResult> ReadAsync(IContext context, ReadOnlyMemory<string> input);
}

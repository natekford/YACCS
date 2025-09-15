using MorseCode.ITask;

using System;

using YACCS.Commands;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a string.
/// </summary>
public class StringTypeReader : TypeReader<string>
{
	/// <inheritdoc />
	public override ITask<ITypeReaderResult<string>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
		=> Success(Join(context, input)).AsITask();
}
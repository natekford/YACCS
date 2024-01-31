using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses an array of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ArrayTypeReader<T> : TypeReader<T[]>
{
	private readonly ListTypeReader<T> _ListTypeReader = new();

	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<T[]>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		// It's simpler to have this class rely on an instance of ListTypeReader
		// We can't use CollectionTypeReader because the actual size is unknown
		var result = await _ListTypeReader.ReadAsync(context, input).ConfigureAwait(false);
		if (!result.InnerResult.IsSuccess)
		{
			return Error(result.InnerResult);
		}
		return Success([.. result.Value!]);
	}
}
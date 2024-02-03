using MorseCode.ITask;

using System.Collections.Immutable;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a <typeparamref name="T"/> via the wrapped type readers.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Creates a new <see cref="AggregateTypeReader{T}"/>.
/// </remarks>
/// <param name="readers">The type readers to wrap.</param>
public sealed class AggregateTypeReader<T>(IEnumerable<ITypeReader<T>> readers)
	: TypeReader<T>
{
	private readonly ImmutableArray<ITypeReader<T>> _Readers
		= readers.ToImmutableArray();

	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<T>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		foreach (var reader in _Readers)
		{
			var result = await reader.ReadAsync(context, input).ConfigureAwait(false);
			if (result.InnerResult.IsSuccess)
			{
				return result;
			}
		}
		return CachedResults<T>.ParseFailed.Result;
	}
}
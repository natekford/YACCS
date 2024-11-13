using MorseCode.ITask;

using System;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;
using YACCS.Parsing;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a string into a <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <param name="input">The input to parse.</param>
/// <param name="result">
/// The resulting value. This may be <see langword="null"/>
/// when <see langword="false"/> is returned.
/// </param>
/// <returns>A bool indicating success or failure.</returns>
public delegate bool TryParseDelegate<TValue>(
	string input,
	[MaybeNullWhen(false)] out TValue result);

/// <summary>
/// Parses a <typeparamref name="TValue"/> via <see cref="TryParseDelegate{TValue}"/>.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <param name="delegate">The delegate to use when parsing.</param>
public class TryParseTypeReader<TValue>(TryParseDelegate<TValue> @delegate)
	: TypeReader<TValue>
{
	private readonly TryParseDelegate<TValue> _Delegate = @delegate;

	/// <inheritdoc />
	public override ITask<ITypeReaderResult<TValue>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		var handler = GetHandler(context.Services);

		if (!_Delegate(handler.Join(input), out var result))
		{
			return CachedResults<TValue>.ParseFailed.Task;
		}
		return Success(result).AsITask();
	}

	[GetServiceMethod]
	private static IArgumentHandler GetHandler(IServiceProvider services)
		=> services.GetRequiredService<IArgumentHandler>();
}
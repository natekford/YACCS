using MorseCode.ITask;

using System;

using YACCS.Commands;
using YACCS.Parsing;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class TypeReader<TValue> : ITypeReader<TValue>
{
	/// <inheritdoc />
	public virtual Type ContextType { get; } = typeof(IContext);
	/// <inheritdoc />
	public Type OutputType { get; } = typeof(TValue);

	/// <inheritdoc />
	public abstract ITask<ITypeReaderResult<TValue>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input);

	ITask<ITypeReaderResult> ITypeReader.ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
		=> ReadAsync(context, input);

	/// <inheritdoc cref="TypeReaderResult{T}.FromError(IResult)"/>
	protected static ITypeReaderResult<TValue> Error(IResult result)
		=> TypeReaderResult<TValue>.FromError(result);

	/// <inheritdoc cref="TypeReaderResult{T}.FromSuccess(T, int?)"/>
	protected static ITypeReaderResult<TValue> Success(TValue value, int? successfullyParsedCount = null)
		=> TypeReaderResult<TValue>.FromSuccess(value, successfullyParsedCount);

	/// <summary>
	/// Joins the strings in <paramref name="input"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="input"></param>
	/// <returns></returns>
	protected virtual string Join(
		IContext context,
		ReadOnlyMemory<string> input)
		=> GetHandler(context.Services).Join(input);

	[GetServiceMethod]
	private static IArgumentHandler GetHandler(IServiceProvider services)
		=> services.GetRequiredService<IArgumentHandler>();
}
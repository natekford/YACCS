using YACCS.Commands;
using YACCS.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Interactivity;

/// <summary>
/// Utilities for interactivity.
/// </summary>
public static class InteractivityUtils
{
	/// <summary>
	/// Extension method for type inference. <paramref name="_"/> is not used.
	/// </summary>
	/// <inheritdoc cref="IInput{TContext, TInput}.GetAsync{TValue}(TContext, InputOptions{TContext, TInput, TValue})"/>
	public static Task<ITypeReaderResult<TValue>> GetAsync<TContext, TInput, TValue>(
		this IInput<TContext, TInput> input,
		TContext context,
		TValue? _,
		InputOptions<TContext, TInput, TValue> options)
		where TContext : IContext
		=> input.GetAsync(context, options);

	internal static IEnumerable<T> ThisOrEmpty<T>(this IEnumerable<T>? enumerable)
		=> enumerable ?? Array.Empty<T>();
}
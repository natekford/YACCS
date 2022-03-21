using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.NamedArguments;

/// <summary>
/// The base class for a named arguments type reader.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NamedArgumentsTypeReaderBase<T> : TypeReader<IContext, T>
	where T : new()
{
	private static readonly char[] _TrimEnd = new[] { ':' };
	private static readonly char[] _TrimStart = new[] { '/', '-' };

	/// <summary>
	/// The parameters this type reader expects.
	/// </summary>
	/// <remarks>
	/// The keys are the current localized parameter name, NOT the original parameter name.
	/// </remarks>
	protected abstract IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<T>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		var dictResult = await TryCreateDictAsync(input).ConfigureAwait(false);
		if (!dictResult.Result.InnerResult.IsSuccess)
		{
			return dictResult.Result;
		}
		return await ReadDictIntoInstanceAsync(context, dictResult.Dictionary).ConfigureAwait(false);
	}

	/// <summary>
	/// Sets a property on <paramref name="instance"/>.
	/// </summary>
	/// <param name="instance">The instance to modify.</param>
	/// <param name="property">The property name.</param>
	/// <param name="value">The value to set.</param>
	protected abstract void SetProperty(T instance, string property, object? value);

	/// <summary>
	/// Attempts to create a dictionary from <paramref name="input"/>.
	/// </summary>
	/// <remarks>
	/// Each odd numbered string in <paramref name="input"/> is a key, each even numbered
	/// string is a value.
	/// </remarks>
	/// <param name="input">The input to create a dictionary from.</param>
	/// <returns>A result indicating success and the created dictionary.</returns>
	protected virtual ValueTask<DictResult> TryCreateDictAsync(ReadOnlyMemory<string> input)
	{
		// Flags aren't supported, so if the input is an odd length
		// we know something is missing
		if (input.Length % 2 != 0)
		{
			return new(new DictResult(CachedResults<T>.NamedArgBadCount.Result, default!));
		}

		var dict = new Dictionary<string, string>();
		for (var i = 0; i < input.Length; i += 2)
		{
			var name = input.Span[i].TrimStart(_TrimStart).TrimEnd(_TrimEnd);
			if (!Parameters.TryGetValue(name, out var parameter))
			{
				return new(new DictResult(Error(new NamedArgNonExistent(name)), dict));
			}

			var property = parameter.ParameterName;
			if (dict.ContainsKey(property))
			{
				return new(new DictResult(Error(new NamedArgDuplicate(name)), dict));
			}
			dict.Add(property, input.Span[i + 1]);
		}
		return new(new DictResult(CachedResults<T>.DefaultSuccess.Result, dict));
	}

	[GetServiceMethod]
	private static IReadOnlyDictionary<Type, ITypeReader> GetReaders(IServiceProvider services)
		=> services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();

	private async ITask<ITypeReaderResult<T>> ReadDictIntoInstanceAsync(
		IContext context,
		IReadOnlyDictionary<string, string> dict)
	{
		var readers = GetReaders(context.Services);

		var instance = new T();
		foreach (var (property, input) in dict)
		{
			var parameter = Parameters[property];

			var reader = readers.GetTypeReader(parameter);
			var result = await reader.ReadAsync(context, new[] { input }).ConfigureAwait(false);
			if (!result.InnerResult.IsSuccess)
			{
				return Error(result.InnerResult);
			}
			SetProperty(instance, parameter.OriginalParameterName, result.Value);
		}
		return Success(instance);
	}

	/// <summary>
	/// Contains a result and a dictionary.
	/// </summary>
	protected readonly struct DictResult
	{
		/// <summary>
		/// The dictionary that was parsed.
		/// </summary>
		public IReadOnlyDictionary<string, string> Dictionary { get; }
		/// <summary>
		/// The result of the parsing.
		/// </summary>
		public ITypeReaderResult<T> Result { get; }

		/// <summary>
		/// Creates a new <see cref="DictResult"/>.
		/// </summary>
		/// <param name="result">
		/// <inheritdoc cref="Result" path="/summary"/>
		/// </param>
		/// <param name="dict">
		/// <inheritdoc cref="Dictionary" path="/summary"/>
		/// </param>
		public DictResult(ITypeReaderResult<T> result, IReadOnlyDictionary<string, string> dict)
		{
			Result = result;
			Dictionary = dict;
		}
	}
}
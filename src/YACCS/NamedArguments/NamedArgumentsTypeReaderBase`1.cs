using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.NamedArguments
{
	public abstract class NamedArgumentsTypeReaderBase<T> : TypeReader<IContext, T>
		where T : new()
	{
		private static readonly char[] _TrimEnd = new[] { ':' };
		private static readonly char[] _TrimStart = new[] { '/', '-' };
		private readonly ConcurrentDictionary<string, ITypeReaderResult<T>> _Duplicate = new();
		private readonly ConcurrentDictionary<string, ITypeReaderResult<T>> _NonExistent = new();

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

		protected abstract void SetProperty(T instance, string property, object? value);

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
					return new(new DictResult(_NonExistent.GetOrAdd(name, static name =>
					{
						return Error(new NamedArgNonExistentResult(name));
					}), dict));
				}

				var property = parameter.ParameterName;
				if (dict.ContainsKey(property))
				{
					return new(new DictResult(_Duplicate.GetOrAdd(property, static property =>
					{
						return Error(new NamedArgDuplicateResult(property));
					}), dict));
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
}
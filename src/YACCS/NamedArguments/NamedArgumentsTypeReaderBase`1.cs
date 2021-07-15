using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.NamedArguments
{
	public abstract class NamedArgumentsTypeReaderBase<T> : TypeReader<T> where T : new()
	{
		private static readonly ITypeReaderResult<T> _ArgCountError
			= TypeReaderResult<T>.FromError(NamedArgBadCountResult.Instance);
		private static readonly char[] _TrimEndChars = new[] { ':' };
		private static readonly char[] _TrimStartChars = new[] { '/', '-' };

		protected abstract IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

		public override ValueTask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (input.Length % 2 != 0)
			{
				return new(_ArgCountError);
			}

			var result = TryCreateDict(input.Span, out var dict);
			if (!result.IsSuccess)
			{
				return new(TypeReaderResult<T>.FromError(result));
			}

			return ReadDictIntoInstanceAsync(context, dict!);
		}

		protected abstract void Setter(T instance, string property, object? value);

		protected virtual IResult TryCreateDict(
			ReadOnlySpan<string> input,
			out IDictionary<string, string> dict)
		{
			dict = new Dictionary<string, string>();
			for (var i = 0; i < input.Length; i += 2)
			{
				var name = input[i].TrimStart(_TrimStartChars).TrimEnd(_TrimEndChars);
				if (!Parameters.TryGetValue(name, out var parameter))
				{
					return new NamedArgNonExistentResult(name);
				}

				var key = parameter.ParameterName;
				if (dict.ContainsKey(key))
				{
					return new NamedArgDuplicateResult(key);
				}
				dict.Add(key, input[i + 1]);
			}
			return SuccessResult.Instance;
		}

		private async ValueTask<ITypeReaderResult<T>> ReadDictIntoInstanceAsync(
			IContext context,
			IDictionary<string, string> dict)
		{
			var registry = context.Services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();

			var instance = new T();
			foreach (var kvp in dict)
			{
				var args = kvp.Value;
				var parameter = Parameters[kvp.Key];
				var reader = registry.GetTypeReader(parameter);

				var result = await reader.ReadAsync(context, args).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess)
				{
					return TypeReaderResult<T>.FromError(result.InnerResult);
				}

				Setter(instance, parameter.OriginalParameterName, result.Value);
			}
			return TypeReaderResult<T>.FromSuccess(instance);
		}
	}
}
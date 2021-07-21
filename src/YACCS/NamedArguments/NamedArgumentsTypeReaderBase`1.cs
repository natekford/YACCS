using System;
using System.Collections.Generic;

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
		protected abstract IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }
		protected char[] TrimEnd { get; set; } = new[] { ':' };
		protected char[] TrimStart { get; set; } = new[] { '/', '-' };

		public override ITask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (input.Length % 2 != 0)
			{
				return CachedResults<T>.NamedArgBadCountTask;
			}

			var result = TryCreateDict(input.Span, out var dict);
			if (!result.IsSuccess)
			{
				return Error(result).AsITask();
			}
			return ReadDictIntoInstanceAsync(context, dict);
		}

		protected abstract void Setter(T instance, string property, object? value);

		protected virtual IResult TryCreateDict(
			ReadOnlySpan<string> input,
			out IReadOnlyDictionary<string, string> dict)
		{
			var mutable = new Dictionary<string, string>();
			dict = mutable;

			for (var i = 0; i < input.Length; i += 2)
			{
				var name = input[i].TrimStart(TrimStart).TrimEnd(TrimEnd);
				if (!Parameters.TryGetValue(name, out var parameter))
				{
					return new NamedArgNonExistentResult(name);
				}

				var property = parameter.ParameterName;
				if (dict.ContainsKey(property))
				{
					return new NamedArgDuplicateResult(property);
				}
				mutable.Add(property, input[i + 1]);
			}
			return SuccessResult.Instance;
		}

		private async ITask<ITypeReaderResult<T>> ReadDictIntoInstanceAsync(
			IContext context,
			IReadOnlyDictionary<string, string> dict)
		{
			var readers = context.Services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();

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
				Setter(instance, parameter.OriginalParameterName, result.Value);
			}
			return Success(instance);
		}
	}
}
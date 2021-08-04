using System;
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
		protected abstract IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }
		protected char[] TrimEnd { get; set; } = new[] { ':' };
		protected char[] TrimStart { get; set; } = new[] { '/', '-' };

		public override async ITask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var (result, dict) = await TryCreateDictAsync(input).ConfigureAwait(false);
			if (!result.InnerResult.IsSuccess)
			{
				return result;
			}
			return await ReadDictIntoInstanceAsync(context, dict).ConfigureAwait(false);
		}

		protected abstract void SetProperty(T instance, string property, object? value);

		protected virtual ValueTask<(ITypeReaderResult<T>, IReadOnlyDictionary<string, string>)>
			TryCreateDictAsync(ReadOnlyMemory<string> input)
		{
			// Flags aren't supported, so if the input is an odd length
			// we know something is missing
			if (input.Length % 2 != 0)
			{
				return new((CachedResults<T>.NamedArgBadCount, default!));
			}

			var dict = new Dictionary<string, string>();
			for (var i = 0; i < input.Length; i += 2)
			{
				var name = input.Span[i].TrimStart(TrimStart).TrimEnd(TrimEnd);
				if (!Parameters.TryGetValue(name, out var parameter))
				{
					return new((Error(new NamedArgNonExistentResult(name)), dict));
				}

				var property = parameter.ParameterName;
				if (dict.ContainsKey(property))
				{
					return new((Error(new NamedArgDuplicateResult(property)), dict));
				}
				dict.Add(property, input.Span[i + 1]);
			}
			return new((Success(default!), dict));
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
					return Error(result);
				}
				SetProperty(instance, parameter.OriginalParameterName, result.Value);
			}
			return Success(instance);
		}
	}
}
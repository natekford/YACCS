using System;
using System.Collections.Generic;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class NullableTypeReader<TValue> : TypeReader<TValue?> where TValue : struct
	{
		private static readonly NullValidator _Null = new();
		private static readonly ITask<ITypeReaderResult<TValue?>> _NullResult
			= TypeReaderResult<TValue?>.FromSuccess(default).AsITask();

		/// <inheritdoc />
		public override ITask<ITypeReaderResult<TValue?>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			var nullValidator = GetNullValidator(context.Services);

			if (nullValidator.IsNull(input!))
			{
				return _NullResult;
			}
			return ReadNonNullAsync(context, input);
		}

		[GetServiceMethod]
		private static INullValidator GetNullValidator(IServiceProvider services)
			=> services.GetService<INullValidator>(_Null);

		[GetServiceMethod]
		private static IReadOnlyDictionary<Type, ITypeReader> GetReaders(IServiceProvider services)
			=> services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();

		private async ITask<ITypeReaderResult<TValue?>> ReadNonNullAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			// We don't need to handle having the correct context type because
			// the type reader we retrieve handles that
			var readers = GetReaders(context.Services);

			var reader = readers.GetTypeReader<TValue>();
			var result = await reader.ReadAsync(context, input).ConfigureAwait(false);
			// We can't just cast result from TRResult<T> to TRResult<Nullable<T>>
			// So we have to recreate the type reader result
			if (!result.InnerResult.IsSuccess)
			{
				return Error(result.InnerResult);
			}
			return Success(result.Value);
		}
	}
}
﻿using MorseCode.ITask;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a collection of <typeparamref name="TElement"/>.
/// </summary>
/// <typeparam name="TElement"></typeparam>
/// <typeparam name="TCollection"></typeparam>
public abstract class CollectionTypeReader<TElement, TCollection> : TypeReader<TCollection>
	where TCollection : ICollection<TElement>
{
	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<TCollection>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		// We don't need to handle having the correct context type because
		// the type reader we retrieve handles that
		var readers = GetReaders(context.Services);
		var handler = GetHandler(context.Services);

		var reader = readers.GetTypeReader<TElement>();
		var values = await CreateCollectionAsync(context, input).ConfigureAwait(false);
		for (var i = 0; i < input.Length; ++i)
		{
			// Try to convert directly from the input string
			var iResult = await reader.ReadAsync(context, input.Slice(i, 1)).ConfigureAwait(false);
			if (iResult.InnerResult.IsSuccess)
			{
				values.Add(iResult.Value!);
				continue;
			}

			// Converting directly from the input string didn't work
			// Try splitting the input string
			if (!handler.TrySplit(input.Span[i], out var args) || args.Length < 2)
			{
				return Error(iResult.InnerResult);
			}

			for (var j = 0; j < args.Length; ++j)
			{
				var jResult = await reader.ReadAsync(context, args.Slice(j, 1)).ConfigureAwait(false);
				if (!jResult.InnerResult.IsSuccess)
				{
					return Error(jResult.InnerResult);
				}
				values.Add(jResult.Value!);
			}
		}
		return Success(values);
	}

	/// <summary>
	/// Creates a collection of <typeparamref name="TElement"/>.
	/// </summary>
	/// <returns>A collection of <typeparamref name="TElement"/>.</returns>
	/// <inheritdoc cref="ReadAsync(IContext, ReadOnlyMemory{string})"/>
	protected abstract ValueTask<TCollection> CreateCollectionAsync(
		IContext context,
		ReadOnlyMemory<string> input);

	[GetServiceMethod]
	private static IArgumentHandler GetHandler(IServiceProvider services)
		=> services.GetRequiredService<IArgumentHandler>();

	[GetServiceMethod]
	private static IReadOnlyDictionary<Type, ITypeReader> GetReaders(IServiceProvider services)
		=> services.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>();
}
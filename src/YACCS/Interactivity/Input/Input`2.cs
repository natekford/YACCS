﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Interactivity.Input
{
	public abstract class Input<TContext, TInput>
		: Interactivity<TContext, TInput>, IInput<TContext, TInput>
		where TContext : IContext
	{
		protected static IImmutableCommand EmptyCommand { get; }
			= new DelegateCommand(
				(Action)(static () => { }),
				new[] { new ImmutableName(new[] { "Input" }) },
				typeof(TContext)
			).ToImmutable();

		protected IReadOnlyDictionary<Type, ITypeReader> TypeReaders { get; }

		protected Input(IReadOnlyDictionary<Type, ITypeReader> typeReaders)
		{
			TypeReaders = typeReaders;
		}

		public virtual Task<ITypeReaderResult<TValue>> GetAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options)
		{
			var eventTrigger = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);
			return HandleInteractionAsync(context, options, eventTrigger, async input =>
			{
				foreach (var criterion in options.Criteria)
				{
					var result = await criterion.JudgeAsync(context, input).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}

				var tr = options.TypeReader ?? TypeReaders.GetTypeReader<TValue>();
				var trResult = await tr.ReadAsync(context, new[] { GetInputString(input) }).ConfigureAwait(false);
				if (!trResult.InnerResult.IsSuccess)
				{
					return trResult.InnerResult;
				}

				var meta = new CommandMeta(EmptyCommand, EmptyParameter<TValue>.Instance);
				foreach (var precondition in options.Preconditions)
				{
					var result = await precondition.CheckAsync(
						meta,
						context,
						trResult.Value
					).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}

				eventTrigger.SetResult(trResult.Value!);
				return SuccessResult.Instance;
			});
		}

		protected abstract string GetInputString(TInput input);

		protected sealed class EmptyParameter<T> : IImmutableParameter
		{
			public static EmptyParameter<T> Instance { get; } = new();

			public IReadOnlyList<object> Attributes { get; } = Array.Empty<object>();
			public object? DefaultValue => null;
			public bool HasDefaultValue => false;
			public int? Length => int.MaxValue;
			public string OriginalParameterName { get; } = $"Input_{typeof(T).FullName}";
			public string ParameterName => OriginalParameterName;
			public Type ParameterType { get; } = typeof(T);
			public IReadOnlyDictionary<string, IReadOnlyList<IParameterPrecondition>> Preconditions { get; }
				= ImmutableDictionary.Create<string, IReadOnlyList<IParameterPrecondition>>();
			public string PrimaryId { get; } = typeof(T).FullName;
			public ITypeReader? TypeReader => null;
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		}
	}
}
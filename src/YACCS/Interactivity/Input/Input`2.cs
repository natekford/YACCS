using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Interactivity.Input
{
	public abstract class Input<TContext, TInput>
		: Interactivity<TContext, TInput>, IInput<TContext, TInput>
		where TContext : IContext
	{
		public IReadOnlyDictionary<Type, ITypeReader> TypeReaders { get; }

		protected static IImmutableCommand EmptyCommand { get; }
			= new DelegateCommand(
				(Action)(() => { }),
				new[] { new ImmutableName(new[] { "Input" }) },
				typeof(TContext)
			).MakeImmutable();

		protected Input(IReadOnlyDictionary<Type, ITypeReader> typeReaders)
		{
			TypeReaders = typeReaders;
		}

		public virtual Task<ITypeReaderResult<TValue>> GetAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options)
		{
			return HandleInteraction<TValue>(context, options, e => new(async i =>
			{
				foreach (var criterion in options.Criteria)
				{
					var result = await criterion.JudgeAsync(context, i).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}

				var tr = options.TypeReader ?? TypeReaders.GetTypeReader<TValue>();
				var trResult = await tr.ReadAsync(context, GetInputString(i)).ConfigureAwait(false);
				if (!trResult.InnerResult.IsSuccess)
				{
					return trResult.InnerResult;
				}

				var value = trResult.Value!;
				var parameterBuilder = new Parameter(typeof(TValue), "InputParameter", null);
				var parameter = parameterBuilder.ToImmutable();
				foreach (var precondition in options.Preconditions)
				{
					var result = await precondition.CheckAsync(EmptyCommand, parameter, context, value).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}

				e.SetResult(trResult.Value!);
				return SuccessResult.Instance.Sync;
			}));
		}

		protected abstract string GetInputString(TInput input);
	}
}
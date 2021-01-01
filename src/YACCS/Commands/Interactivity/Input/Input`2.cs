using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity.Input
{
	public abstract class Input<TContext, TInput>
		: Interactivity<TContext, TInput>, IInput<TContext, TInput>
		where TContext : IContext
	{
		public ITypeRegistry<ITypeReader> TypeReaders { get; }

		protected static Delegate EmptyDelegate { get; } = (Action)(() => { });
		protected static IEnumerable<IName> EmptyNames { get; } = new[] { new Name(new[] { "Input" }) };

		protected Input(ITypeRegistry<ITypeReader> typeReaders)
		{
			TypeReaders = typeReaders;
		}

		public virtual Task<ITypeReaderResult<TValue>> GetAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options)
		{
			return HandleInteraction<TValue>(context, options, e => new OnInput(async i =>
			{
				foreach (var criterion in options.Criteria)
				{
					var result = await criterion.JudgeAsync(context, i).ConfigureAwait(false);
					if (!result)
					{
						return FailureResult.Instance.Sync;
					}
				}

				var tr = options.TypeReader ?? TypeReaders.Get<TValue>();
				var trResult = await tr.ReadAsync(context, GetInputString(i)).ConfigureAwait(false);
				if (!trResult.InnerResult.IsSuccess)
				{
					return trResult.InnerResult;
				}

				var value = trResult.Value!;
				var parameterBuilder = new Parameter(typeof(TValue), "InputParameter", null);
				var parameter = parameterBuilder.ToImmutable(null);
				foreach (var precondition in options.Preconditions)
				{
					var result = await precondition.CheckAsync(parameter, context, value).ConfigureAwait(false);
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
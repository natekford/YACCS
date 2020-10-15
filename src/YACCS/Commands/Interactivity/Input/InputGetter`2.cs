using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity.Input
{
	public abstract class InputGetter<TContext, TInput>
		: InteractiveBase<TContext, TInput>, IInputGetter<TContext, TInput>
		where TContext : IContext
	{
		public ITypeRegistry<ITypeReader> TypeReaders { get; }

		protected static Delegate EmptyDelegate { get; } = (Action)(() => { });
		protected static IEnumerable<IName> EmptyNames { get; } = new[] { new Name(new[] { "Input" }) };

		protected InputGetter(ITypeRegistry<ITypeReader> typeReaders)
		{
			TypeReaders = typeReaders;
		}

		public virtual Task<IInteractiveResult<TValue>> GetInputAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options)
		{
			return GetInputAsync<TValue>(context, options, e => new OnInput(async i =>
			{
				foreach (var criterion in options.Criteria)
				{
					var result = await criterion.JudgeAsync(context, i).ConfigureAwait(false);
					if (!result)
					{
						return;
					}
				}

				var tr = options.TypeReader ?? TypeReaders.GetReader<TValue>();
				var trResult = await tr.ReadAsync(context, GetInputString(i)).ConfigureAwait(false);
				if (!trResult.IsSuccess)
				{
					return;
				}

				var value = trResult.Value!;
				var parameter = GenerateInputParameter<TValue>();
				foreach (var precondition in options.Preconditions)
				{
					var result = await precondition.CheckAsync(parameter, context, value).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return;
					}
				}

				e.SetResult(trResult.Value!);
			}));
		}

		protected static ParameterInfo GenerateInputParameter<TValue>()
		{
			var commandBuilder = new DelegateCommand(EmptyDelegate, EmptyNames);
			var parameterBuilder = new Parameter(typeof(TValue), "InputParameter", null);
			commandBuilder.Parameters.Add(parameterBuilder);

			var command = commandBuilder.ToImmutable().Single();
			var parameter = command.Parameters[0];
			return new ParameterInfo(command, parameter);
		}

		protected abstract string GetInputString(TInput input);
	}
}
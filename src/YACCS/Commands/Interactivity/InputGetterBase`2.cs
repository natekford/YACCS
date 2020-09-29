using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public abstract class InputGetterBase<TContext, TInput> : IInputGetter<TContext, TInput>
		where TContext : IContext
	{
		public virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);
		public ITypeReaderRegistry TypeReaders { get; }

		protected static Delegate EmptyDelegate { get; } = (Action)(() => { });
		protected static IEnumerable<IName> EmptyNames { get; } = new[] { new Name(new[] { "Input" }) };

		protected InputGetterBase(ITypeReaderRegistry typeReaders)
		{
			TypeReaders = typeReaders;
		}

		public async Task<IInteractiveResult<TValue>> GetInputAsync<TValue>(
			TContext context,
			IGetInputOptions<TContext, TInput, TValue>? options = null)
		{
			var eventTrigger = new TaskCompletionSource<TValue>();
			var cancelTrigger = new TaskCompletionSource<bool>();
			if (options?.Token is CancellationToken token)
			{
				token.Register(() => cancelTrigger.SetResult(true));
			}

			var handler = CreateOnInputDelegate(context, eventTrigger, options);
			// Since delegate equality is kind of wonky, let's just use a guid id in case subscribe
			// creates a new delegate/closure from the OnInput delegate so the dev can store the new
			// delegate/closure in a dictionary and retrieve it to unsubscribe with the correct one
			var id = Guid.NewGuid();
			Subscribe(context, handler, id);
			var @event = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(options?.Timeout ?? DefaultTimeout);
			var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
			Unsubscribe(context, handler, id);

			if (task == cancel)
			{
				return new InteractiveResult<TValue>(CanceledResult.Instance);
			}
			if (task == delay)
			{
				return new InteractiveResult<TValue>(TimedOutResult.Instance);
			}

			var value = await @event.ConfigureAwait(false);
			return new InteractiveResult<TValue>(value);
		}

		protected static ParameterInfo GenerateInputParameter<TValue>()
		{
			var commandBuilder = new DelegateCommand(EmptyDelegate, EmptyNames);
			var parameterBuilder = new Parameter(typeof(TValue), "InputParameter");
			commandBuilder.Parameters.Add(parameterBuilder);

			var command = commandBuilder.ToCommand();
			var parameter = command.Parameters[0];
			return new ParameterInfo(command, parameter);
		}

		protected virtual OnInput<TValue> CreateOnInputDelegate<TValue>(
			TContext context,
			TaskCompletionSource<TValue> eventTrigger,
			IGetInputOptions<TContext, TInput, TValue>? options = null)
		{
			var parameter = GenerateInputParameter<TValue>();
			var criteria = options?.Criteria ?? Array.Empty<ICriterion<TContext, TInput>>();
			var typeReader = options?.TypeReader ?? TypeReaders.GetReader<TValue>();
			var preconditions = options?.Preconditions ?? Array.Empty<IParameterPrecondition<TContext, TValue>>();

			return new OnInput<TValue>(async input =>
			{
				foreach (var criterion in criteria)
				{
					var result = await criterion.JudgeAsync(context, input).ConfigureAwait(false);
					if (!result)
					{
						return;
					}
				}

				var inputString = GetInputString(input);
				var trResult = await typeReader.ReadAsync(context, inputString).ConfigureAwait(false);
				if (!trResult.IsSuccess)
				{
					return;
				}
				var value = trResult.Arg;

				foreach (var precondition in preconditions)
				{
					var result = await precondition.CheckAsync(parameter, context, value!).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return;
					}
				}

				eventTrigger.SetResult(trResult.Arg!);
			});
		}

		protected abstract string GetInputString(TInput input);

		protected abstract void Subscribe<TValue>(TContext context, OnInput<TValue> onInput, Guid id);

		protected abstract void Unsubscribe<TValue>(TContext context, OnInput<TValue> onInput, Guid id);

		protected delegate Task OnInput<TValue>(TInput input);
	}
}
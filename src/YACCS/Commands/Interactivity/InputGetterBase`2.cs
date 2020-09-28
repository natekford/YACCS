using System;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public abstract class InputGetterBase<TContext, TInput> : IInputGetter<TContext, TInput>
		where TContext : IContext
	{
		public virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);
		public ITypeReaderRegistry TypeReaders { get; }

		protected InputGetterBase(ITypeReaderRegistry typeReaders)
		{
			TypeReaders = typeReaders;
		}

		public async Task<IInteractiveResult<TValue>> GetInputAsync<TValue>(
			TContext context,
			IGetInputOptions<TContext, TInput, TValue> options)
		{
			var timeout = options?.Timeout ?? DefaultTimeout;
			var criteria = options?.Criteria ?? Array.Empty<ICriterion<TContext, TInput>>();
			var typeReader = options?.TypeReader ?? TypeReaders.GetReader<TValue>();

			var eventTrigger = new TaskCompletionSource<TValue>();
			var cancelTrigger = new TaskCompletionSource<bool>();
			if (options?.Token is CancellationToken token)
			{
				token.Register(() => cancelTrigger.SetResult(true));
			}

			async Task Handler(TInput input)
			{
				foreach (var criterion in criteria!)
				{
					var result = await criterion.JudgeAsync(context, input).ConfigureAwait(false);
					if (!result)
					{
						return;
					}
				}

				var inputString = GetInputString(input);
				var trResult = await typeReader!.ReadAsync(context, inputString).ConfigureAwait(false);
				if (trResult.IsSuccess)
				{
					eventTrigger!.SetResult(trResult.Arg!);
				}
			}

			var handler = CreateInputDelegate(Handler);
			Subscribe(context, handler);
			var @event = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(timeout);
			var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
			Unsubscribe(context, handler);

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

		protected virtual OnNextValue CreateInputDelegate(Func<TInput, Task> handler)
					=> new OnNextValue(handler);

		protected abstract string GetInputString(TInput input);

		protected abstract void Subscribe(TContext context, OnNextValue onNextValue);

		protected abstract void Unsubscribe(TContext context, OnNextValue onNextValue);

		protected delegate Task OnNextValue(TInput input);
	}
}
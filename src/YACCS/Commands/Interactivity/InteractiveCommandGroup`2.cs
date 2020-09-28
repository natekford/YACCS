﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity
{
	public abstract class InteractiveCommandGroup<TContext, TInput> : CommandGroup<TContext>
		where TContext : IContext
	{
		public virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);
		public ITypeReaderRegistry TypeReaders { get; set; } = null!;

		#region Input

		public virtual OnNextValue CreateInputDelegate(Func<TInput, Task> handler)
			=> new OnNextValue(handler);

		public async Task<IInteractiveResult<TValue>> GetInputAsync<TValue>(NextValueOptions<TValue> options)
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
					var result = await criterion.JudgeAsync(Context, input).ConfigureAwait(false);
					if (!result)
					{
						return;
					}
				}

				var inputString = GetInputString(input);
				var trResult = await typeReader!.ReadAsync(Context, inputString).ConfigureAwait(false);
				if (trResult.IsSuccess)
				{
					eventTrigger!.SetResult(trResult.Arg!);
				}
			}

			var handler = CreateInputDelegate(Handler);
			SubscribeForInput(handler);
			var @event = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(timeout);
			var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
			UnsubscribeForInput(handler);

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

		public abstract string GetInputString(TInput input);

		public abstract void SubscribeForInput(OnNextValue onNextValue);

		public abstract void UnsubscribeForInput(OnNextValue onNextValue);

		#endregion Input

		#region Pagination

		public virtual OnNextValue CreatePaginationDelegate(Func<TInput, Task> handler)
			=> new OnNextValue(handler);

		// TODO

		public abstract void SubscribeForPagination(OnNextValue onNextValue);

		public abstract void UnsubscribeForPagination(OnNextValue onNextValue);

		#endregion Pagination

		public delegate Task OnNextValue(TInput input);

		public class NextValueOptions<TOutput>
		{
			public IEnumerable<ICriterion<TContext, TInput>>? Criteria { get; set; }
			public TimeSpan? Timeout { get; set; }
			public CancellationToken? Token { get; set; }
			public ITypeReader<TOutput>? TypeReader { get; set; }
		}
	}
}
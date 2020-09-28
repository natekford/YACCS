using System;
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
		public ITypeReaderRegistry TypeReaders { get; set; } = null!;

		#region Next Value

		public virtual NextValue CreateNextValueDelegate(Func<TInput, Task> handler)
			=> new NextValue(handler);

		public abstract string GetInputString(TInput input);

		public async Task<IInteractiveResult<TValue>> NextValueAsync<TValue>(NextValueOptions<TValue> options)
		{
			var timeout = options?.Timeout ?? TimeSpan.FromSeconds(3);
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

			var handler = CreateNextValueDelegate(Handler);
			Subscribe(handler);
			var @event = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(timeout);
			var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
			Unsubscribe(handler);

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

		public abstract void Subscribe(NextValue onInputReceived);

		public abstract void Unsubscribe(NextValue onInputReceived);

		public delegate Task NextValue(TInput input);

		public class NextValueOptions<TOutput>
		{
			public IEnumerable<ICriterion<TContext, TInput>>? Criteria { get; set; }
			public TimeSpan? Timeout { get; set; }
			public CancellationToken? Token { get; set; }
			public ITypeReader<TOutput>? TypeReader { get; set; }
		}

		#endregion Next Value

		#region Pagination

		// TODO

		#endregion Pagination
	}
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public class ConsoleInputGetter : InputGetter<ConsoleContext, string>
	{
		private readonly Dictionary<Guid, CancellationTokenSource> _Input
			= new Dictionary<Guid, CancellationTokenSource>();

		public ConsoleInputGetter(ITypeRegistry<ITypeReader> registry) : base(registry)
		{
		}

		protected override string GetInputString(string input)
			=> input;

		protected override void Subscribe(ConsoleContext context, OnInput onInput)
		{
			context.Services.GetRequiredService<SemaphoreSlim>().Wait();

			var source = new CancellationTokenSource();
			Task.Run(async () =>
			{
				// This may seem extremely convoluted and unnecessarily nested, but it's not

				// First, only keep the loop going when not canceled
				while (!source.IsCancellationRequested)
				{
					// Second, use Console.KeyAvailable to have Console.ReadLine not be blocking
					// I'm not sure why, but without this if statement Console.ReadLine acts
					// very differently
					if (!Console.KeyAvailable)
					{
						continue;
					}

					var input = Console.ReadLine();
					// Third, even though we have the loop condition already checking this,
					// check it again so we don't invoke the delegate after timeout/cancel
					if (source.IsCancellationRequested)
					{
						return;
					}

					var result = await onInput.Invoke(input).ConfigureAwait(false);
					context.Services.GetRequiredService<ConsoleWriter>().WriteResponse(result);
				}
			});
			_Input[context.Id] = source;
		}

		protected override void Unsubscribe(ConsoleContext context, OnInput onInput)
		{
			context.Services.GetRequiredService<SemaphoreSlim>().Release();

			_Input[context.Id].Cancel();
		}
	}
}
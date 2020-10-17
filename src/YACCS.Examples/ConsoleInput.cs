﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public class ConsoleInput : Input<IContext, string>
	{
		private readonly Dictionary<Guid, CancellationTokenSource> _Input
			= new Dictionary<Guid, CancellationTokenSource>();
		private readonly SemaphoreSlim _InputSemaphore;
		private readonly SemaphoreSlim _OutputSemaphore;
		private readonly ConsoleWriter _Writer;

		public ConsoleInput(
			ITypeRegistry<ITypeReader> registry,
			SemaphoreSlim inputSemaphore,
			SemaphoreSlim outputSemaphore,
			ConsoleWriter writer) : base(registry)
		{
			_InputSemaphore = inputSemaphore;
			_OutputSemaphore = outputSemaphore;
			_Writer = writer;
		}

		protected override string GetInputString(string input)
			=> input;

		protected override async Task SubscribeAsync(IContext context, OnInput onInput)
		{
			await _OutputSemaphore.WaitAsync().ConfigureAwait(false);
			await _InputSemaphore.WaitAsync().ConfigureAwait(false);

			var source = new CancellationTokenSource();
			_ = Task.Run(async () =>
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
					_Writer.WriteResult(result);
				}
			});
			_Input[context.Id] = source;
		}

		protected override Task UnsubscribeAsync(IContext context, OnInput onInput)
		{
			_InputSemaphore.ReleaseIfZero();
			_Input[context.Id].Cancel();
			return Task.CompletedTask;
		}
	}
}
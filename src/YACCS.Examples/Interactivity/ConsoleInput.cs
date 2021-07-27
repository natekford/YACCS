﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Interactivity;
using YACCS.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Examples.Interactivity
{
	public sealed class ConsoleInput : Input<ConsoleContext, string>
	{
		private readonly ConsoleInteractivityManager _Interactivity;

		public ConsoleInput(
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ConsoleInteractivityManager interactivity)
			: base(readers)
		{
			_Interactivity = interactivity;
		}

		protected override string GetInputString(string input)
			=> input;

		protected override Task SubscribeAsync(ConsoleContext context, OnInput<string> onInput)
			=> _Interactivity.SubscribeAsync(context, onInput);

		protected override Task UnsubscribeAsync(ConsoleContext context, OnInput<string> onInput)
			=> _Interactivity.UnsubscribeAsync(context, onInput);
	}
}
﻿using System.Threading.Tasks;

using YACCS.Interactivity;
using YACCS.Interactivity.Pagination;

namespace YACCS.Examples.Interactivity
{
	public sealed class ConsolePaginator : Paginator<ConsoleContext, string>
	{
		private readonly ConsoleInteractivityManager _Interactivity;

		public ConsolePaginator(ConsoleInteractivityManager interactivity)
		{
			_Interactivity = interactivity;
		}

		protected override Task<int?> ConvertAsync(string input)
			=> Task.FromResult<int?>(int.TryParse(input, out var result) ? result : null);

		protected override int GetNewPage(int current, int max, int diff)
			=> diff;

		protected override Task SubscribeAsync(ConsoleContext context, OnInput<string> onInput)
			=> _Interactivity.SubscribeAsync(context, onInput);

		protected override Task UnsubscribeAsync(ConsoleContext context, OnInput<string> onInput)
			=> _Interactivity.UnsubscribeAsync(context, onInput);
	}
}
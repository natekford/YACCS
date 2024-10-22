using YACCS.Interactivity;
using YACCS.Interactivity.Pagination;

namespace YACCS.Examples.Interactivity;

public sealed class ConsolePaginator(ConsoleHandler console)
	: Paginator<ConsoleContext, string>
{
	private readonly ConsoleHandler _Console = console;

	protected override Task<int?> ConvertAsync(string input)
		=> Task.FromResult<int?>(int.TryParse(input, out var result) ? result : null);

	protected override Task<IAsyncDisposable> SubscribeAsync(
		ConsoleContext _,
		OnInput<string> onInput)
		=> _Console.SubscribeAsync(onInput);
}
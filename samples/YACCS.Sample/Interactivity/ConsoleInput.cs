using YACCS.Interactivity;
using YACCS.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Sample.Interactivity;

public sealed class ConsoleInput(
	IReadOnlyDictionary<Type, ITypeReader> readers,
	ConsoleHandler console)
	: Input<ConsoleContext, string>(readers)
{
	private readonly ConsoleHandler _Console = console;

	protected override string GetInputString(string input)
		=> input;

	protected override Task<IAsyncDisposable> SubscribeAsync(
		ConsoleContext _,
		OnInput<string> onInput)
		=> _Console.SubscribeAsync(onInput);
}
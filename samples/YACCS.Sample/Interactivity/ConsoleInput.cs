using YACCS.Interactivity;
using YACCS.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Examples.Interactivity;

public sealed class ConsoleInput(
	IReadOnlyDictionary<Type, ITypeReader> readers,
	ConsoleInteractivityManager interactivity)
	: Input<ConsoleContext, string>(readers)
{
	private readonly ConsoleInteractivityManager _Interactivity = interactivity;

	protected override string GetInputString(string input)
		=> input;

	protected override Task<IAsyncDisposable> SubscribeAsync(
		ConsoleContext _,
		OnInput<string> onInput)
		=> _Interactivity.SubscribeAsync(onInput);
}
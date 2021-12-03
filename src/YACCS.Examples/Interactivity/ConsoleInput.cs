using YACCS.Interactivity;
using YACCS.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Examples.Interactivity;

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

	protected override Task<IAsyncDisposable> SubscribeAsync(
		ConsoleContext _,
		OnInput<string> onInput)
		=> _Interactivity.SubscribeAsync(onInput);
}

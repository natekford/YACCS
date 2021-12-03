using YACCS.Commands;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Interactivity.Input;

/// <summary>
/// Interactivity options which support reading values from input.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TValue"></typeparam>
public interface IInputOptions<in TContext, in TInput, TValue> :
	IInteractivityOptions<TContext, TInput>
	where TContext : IContext
{
	/// <summary>
	/// Used for validating the parsed vaue.
	/// </summary>
	IEnumerable<IParameterPrecondition<TContext, TValue>> Preconditions { get; }
	/// <summary>
	/// Used for parsing a value from the input.
	/// </summary>
	ITypeReader<TValue>? TypeReader { get; }
}
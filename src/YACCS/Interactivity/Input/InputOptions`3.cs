using System.Collections.Generic;

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
/// <param name="Preconditions">Used for validating the parsed vaue.</param>
/// <param name="TypeReader">Used for parsing a value from the input.</param>
public record InputOptions<TContext, TInput, TValue>(
	IEnumerable<IParameterPrecondition<TContext, TValue>>? Preconditions = null,
	ITypeReader<TValue>? TypeReader = null
) : InteractivityOptions<TContext, TInput> where TContext : IContext;
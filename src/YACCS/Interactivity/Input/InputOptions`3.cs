using YACCS.Commands;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Interactivity.Input;

/// <inheritdoc cref="IInputOptions{TContext, TInput, TValue}"/>
public record InputOptions<TContext, TInput, TValue>(
	IEnumerable<ICriterion<TContext, TInput>>? Criteria = null,
	IEnumerable<IParameterPrecondition<TContext, TValue>>? Preconditions = null,
	TimeSpan? Timeout = null,
	CancellationToken? Token = null,
	ITypeReader<TValue>? TypeReader = null
) : IInputOptions<TContext, TInput, TValue> where TContext : IContext;
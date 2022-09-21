using YACCS.Commands;

namespace YACCS.Interactivity;

/// <summary>
/// Interactivity options which support validating input.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TInput"></typeparam>
/// <param name="Criteria">Criteria for determining if an input source is valid.</param>
/// <param name="Timeout">How long to wait before timing out.</param>
/// <param name="Token">Token used for cancellation.</param>
public record InteractivityOptions<TContext, TInput>(
	IEnumerable<ICriterion<TContext, TInput>>? Criteria = null,
	TimeSpan? Timeout = null,
	CancellationToken? Token = null
) where TContext : IContext;
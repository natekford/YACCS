
using YACCS.Commands;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Interactivity.Input
{
	/// <inheritdoc cref="IInputOptions{TContext, TInput, TValue}"/>
	public class InputOptions<TContext, TInput, TValue>
		: IInputOptions<TContext, TInput, TValue>
		where TContext : IContext
	{
		/// <inheritdoc />
		public IEnumerable<ICriterion<TContext, TInput>> Criteria { get; set; }
			= Array.Empty<ICriterion<TContext, TInput>>();
		/// <inheritdoc />
		public IEnumerable<IParameterPrecondition<TContext, TValue>> Preconditions { get; set; }
			= Array.Empty<IParameterPrecondition<TContext, TValue>>();
		/// <inheritdoc />
		public TimeSpan? Timeout { get; set; }
		/// <inheritdoc />
		public CancellationToken? Token { get; set; }
		/// <inheritdoc />
		public ITypeReader<TValue>? TypeReader { get; set; }
	}
}
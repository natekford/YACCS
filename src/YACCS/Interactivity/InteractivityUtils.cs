
using YACCS.Commands;
using YACCS.Interactivity.Input;
using YACCS.Interactivity.Pagination;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Interactivity
{
	/// <summary>
	/// Utilities for interactivity.
	/// </summary>
	public static class InteractivityUtils
	{
		/// <summary>
		/// Creates a new <see cref="InputOptionsFactory{TContext, TInput}"/>.
		/// </summary>
		/// <typeparam name="TContext"></typeparam>
		/// <typeparam name="TInput"></typeparam>
		/// <param name="_"></param>
		/// <returns>
		/// <inheritdoc cref="InputOptionsFactory{TContext, TInput}" path="/summary"/>
		/// </returns>
		public static InputOptionsFactory<TContext, TInput> CreateOptions<TContext, TInput>(
			this IInput<TContext, TInput> _)
			where TContext : IContext
			=> new();

		/// <summary>
		/// Creates a new <see cref="PageOptionsFactory{TContext, TInput}"/>.
		/// </summary>
		/// <typeparam name="TContext"></typeparam>
		/// <typeparam name="TInput"></typeparam>
		/// <param name="_"></param>
		/// <returns>
		/// <inheritdoc cref="PageOptionsFactory{TContext, TInput}" path="/summary"/>
		/// </returns>
		public static PageOptionsFactory<TContext, TInput> CreateOptions<TContext, TInput>(
			this IPaginator<TContext, TInput> _)
			where TContext : IContext
			=> new();

		/// <summary>
		/// A factory for creating <see cref="InputOptions{TContext, TInput, TValue}"/>.
		/// </summary>
		/// <typeparam name="TContext"></typeparam>
		/// <typeparam name="TInput"></typeparam>
		public readonly struct InputOptionsFactory<TContext, TInput> where TContext : IContext
		{
			/// <summary>
			/// Creates a new <see cref="InputOptions{TContext, TInput, TValue}"/>.
			/// </summary>
			/// <typeparam name="TValue"></typeparam>
			/// <returns>A new instance of input options.</returns>
			public InputOptions<TContext, TInput, TValue> For<TValue>()
				=> new();

			/// <summary>
			/// Creates a new <see cref="InputOptions{TContext, TInput, TValue}"/>.
			/// </summary>
			/// <typeparam name="TValue"></typeparam>
			/// <param name="criteria">
			/// <inheritdoc cref="InputOptions{TContext, TInput, TValue}.Criteria" path="/summary"/>
			/// </param>
			/// <param name="preconditions">
			/// <inheritdoc cref="InputOptions{TContext, TInput, TValue}.Preconditions" path="/summary"/>
			/// </param>
			/// <param name="timeout">
			/// <inheritdoc cref="InputOptions{TContext, TInput, TValue}.Timeout" path="/summary"/>
			/// </param>
			/// <param name="token">
			/// <inheritdoc cref="InputOptions{TContext, TInput, TValue}.Token" path="/summary"/>
			/// </param>
			/// <param name="typeReader">
			/// <inheritdoc cref="InputOptions{TContext, TInput, TValue}.TypeReader" path="/summary"/>
			/// </param>
			/// <returns>An instance of input options with the supplied arguments.</returns>
			public InputOptions<TContext, TInput, TValue> With<TValue>(
				IEnumerable<ICriterion<TContext, TInput>>? criteria = null,
				IEnumerable<IParameterPrecondition<TContext, TValue>>? preconditions = null,
				TimeSpan? timeout = null,
				CancellationToken? token = null,
				ITypeReader<TValue>? typeReader = null)
			{
				var options = new InputOptions<TContext, TInput, TValue>();
				options.Criteria = criteria ?? options.Criteria;
				options.Preconditions = preconditions ?? options.Preconditions;
				options.Timeout = timeout ?? options.Timeout;
				options.Token = token ?? options.Token;
				options.TypeReader = typeReader ?? options.TypeReader;
				return options;
			}
		}

		/// <summary>
		/// A factory for creating <see cref="PaginatorOptions{TContext, TInput}"/>.
		/// </summary>
		/// <typeparam name="TContext"></typeparam>
		/// <typeparam name="TInput"></typeparam>
		public readonly struct PageOptionsFactory<TContext, TInput> where TContext : IContext
		{
			/// <summary>
			/// Creates a new <see cref="PaginatorOptions{TContext, TInput}"/>.
			/// </summary>
			/// <param name="maxPage">
			/// <inheritdoc cref="PaginatorOptions{TContext, TInput}.MaxPage" path="/summary"/>
			/// </param>
			/// <param name="displayCallback">
			/// <inheritdoc cref="PaginatorOptions{TContext, TInput}.DisplayCallback" path="/summary"/>
			/// </param>
			/// <param name="criteria">
			/// <inheritdoc cref="PaginatorOptions{TContext, TInput}.Criteria" path="/summary"/>
			/// </param>
			/// <param name="startingPage">
			/// <inheritdoc cref="PaginatorOptions{TContext, TInput}.StartingPage" path="/summary"/>
			/// </param>
			/// <param name="timeout">
			/// <inheritdoc cref="PaginatorOptions{TContext, TInput}.Timeout" path="/summary"/>
			/// </param>
			/// <param name="token">
			/// <inheritdoc cref="PaginatorOptions{TContext, TInput}.Token" path="/summary"/>
			/// </param>
			/// <returns>An instance of paginator options with the supplied arguments.</returns>
			public PaginatorOptions<TContext, TInput> With(
				int maxPage,
				Func<int, Task> displayCallback,
				IEnumerable<ICriterion<TContext, TInput>>? criteria = null,
				int? startingPage = null,
				TimeSpan? timeout = null,
				CancellationToken? token = null)
			{
				var options = new PaginatorOptions<TContext, TInput>(maxPage, displayCallback);
				options.Criteria = criteria ?? options.Criteria;
				options.StartingPage = startingPage ?? options.StartingPage;
				options.Timeout = timeout ?? options.Timeout;
				options.Token = token ?? options.Token;
				return options;
			}
		}
	}
}
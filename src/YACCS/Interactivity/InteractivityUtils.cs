using System;
using System.Collections.Generic;
using System.Threading;

using YACCS.Commands;
using YACCS.Interactivity.Input;
using YACCS.Interactivity.Pagination;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Interactivity
{
	public static class InteractivityUtils
	{
		public static InputOptionsFactory<TContext, TInput> CreateOptions<TContext, TInput>(
			this IInput<TContext, TInput> _)
			where TContext : IContext
			=> new();

		public static PageOptionsFactory<TContext, TInput> CreateOptions<TContext, TInput>(
			this IPaginator<TContext, TInput> _)
			where TContext : IContext
			=> new();

		public readonly struct InputOptionsFactory<TContext, TInput> where TContext : IContext
		{
			public InputOptions<TContext, TInput, TValue> For<TValue>()
				=> new();

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

		public readonly struct PageOptionsFactory<TContext, TInput> where TContext : IContext
		{
			public PageOptions<TContext, TInput> With(
				int maxPage,
				IEnumerable<ICriterion<TContext, TInput>>? criteria = null,
				int? startingPage = null,
				TimeSpan? timeout = null,
				CancellationToken? token = null)
			{
				var options = new PageOptions<TContext, TInput>();
				options.Criteria = criteria ?? options.Criteria;
				options.MaxPage = maxPage;
				options.StartingPage = startingPage ?? options.StartingPage;
				options.Timeout = timeout ?? options.Timeout;
				options.Token = token ?? options.Token;
				return options;
			}
		}
	}
}
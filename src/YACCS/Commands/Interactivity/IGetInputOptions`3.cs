using System;
using System.Collections.Generic;
using System.Threading;

using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity
{
	public interface IGetInputOptions<TContext, TInput, TValue> where TContext : IContext
	{
		IEnumerable<ICriterion<TContext, TInput>>? Criteria { get; }
		IEnumerable<IParameterPrecondition<TContext, TValue>>? Preconditions { get; }
		TimeSpan? Timeout { get; }
		CancellationToken? Token { get; }
		ITypeReader<TValue>? TypeReader { get; }
	}
}
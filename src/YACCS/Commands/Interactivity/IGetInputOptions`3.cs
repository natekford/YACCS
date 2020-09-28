using System;
using System.Collections.Generic;
using System.Threading;

using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity
{
	public interface IGetInputOptions<TContext, TInput, TOutput> where TContext : IContext
	{
		IEnumerable<ICriterion<TContext, TInput>>? Criteria { get; set; }
		TimeSpan? Timeout { get; set; }
		CancellationToken? Token { get; set; }
		ITypeReader<TOutput>? TypeReader { get; set; }
	}
}
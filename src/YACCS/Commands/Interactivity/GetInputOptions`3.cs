using System;
using System.Collections.Generic;
using System.Threading;

using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity
{
	public class GetInputOptions<TContext, TInput, TOutput> : IGetInputOptions<TContext, TInput, TOutput>
		where TContext : IContext
	{
		public IEnumerable<ICriterion<TContext, TInput>>? Criteria { get; set; }
		public TimeSpan? Timeout { get; set; }
		public CancellationToken? Token { get; set; }
		public ITypeReader<TOutput>? TypeReader { get; set; }
	}
}
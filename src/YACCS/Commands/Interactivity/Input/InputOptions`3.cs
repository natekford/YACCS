using System;
using System.Collections.Generic;
using System.Threading;

using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity.Input
{
	public class InputOptions<TContext, TInput, TValue> : IInputOptions<TContext, TInput, TValue>
		where TContext : IContext
	{
		public IEnumerable<ICriterion<TContext, TInput>>? Criteria { get; set; }
		public IEnumerable<IParameterPrecondition<TContext, TValue>>? Preconditions { get; set; }
		public TimeSpan? Timeout { get; set; }
		public CancellationToken? Token { get; set; }
		public ITypeReader<TValue>? TypeReader { get; set; }
	}
}
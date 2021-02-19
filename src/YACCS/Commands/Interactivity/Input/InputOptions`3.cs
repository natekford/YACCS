﻿using System;
using System.Collections.Generic;
using System.Threading;

using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity.Input
{
	public class InputOptions<TContext, TInput, TValue> : IInputOptions<TContext, TInput, TValue>
		where TContext : IContext
	{
		public IEnumerable<ICriterion<TContext, TInput>> Criteria { get; set; }
			= Array.Empty<ICriterion<TContext, TInput>>();
		public IEnumerable<IParameterPrecondition<TContext, TValue>> Preconditions { get; set; }
			= Array.Empty<IParameterPrecondition<TContext, TValue>>();
		public TimeSpan? Timeout { get; set; }
		public CancellationToken? Token { get; set; }
		public ITypeReader<TValue>? TypeReader { get; set; }
	}
}
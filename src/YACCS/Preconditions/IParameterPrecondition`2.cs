﻿using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IParameterPrecondition<in TContext, in TValue>
		: IParameterPrecondition<TValue>
		where TContext : IContext
	{
		ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			TContext context,
			TValue? value);
	}
}
﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IParameterPrecondition<in TValue> : IParameterPrecondition
	{
		Task<IResult> CheckAsync(
			IImmutableParameter parameter,
			IContext context,
			[MaybeNull] TValue value);
	}
}
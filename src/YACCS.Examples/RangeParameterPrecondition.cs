﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples
{
	public class RangeParameterPrecondition : ParameterPrecondition<IContext, int>
	{
		private readonly int _Max;
		private readonly int _Min;
		private readonly IResult _TooHigh;
		private readonly IResult _TooLow;

		public RangeParameterPrecondition(int min, int max)
		{
			_Max = max;
			_TooHigh = new FailureResult($"Must be less than or equal to {_Max}.");
			_Min = min;
			_TooLow = new FailureResult($"Must be greater or equal to {_Min}.");
		}

		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			[MaybeNull] int value)
		{
			if (value < _Min)
			{
				return new(_TooLow);
			}
			if (value > _Max)
			{
				return new(_TooHigh);
			}
			return new(SuccessResult.Instance);
		}
	}
}
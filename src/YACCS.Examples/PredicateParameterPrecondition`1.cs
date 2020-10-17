using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Results;

namespace YACCS.Examples
{
	public class PredicateParameterPrecondition<T> : ParameterPrecondition<ConsoleContext, T>
	{
		private readonly Func<T, bool> _Predicate;

		public PredicateParameterPrecondition(Func<T, bool> predicate)
		{
			_Predicate = predicate;
		}

		public override Task<IResult> CheckAsync(
			ParameterInfo parameter,
			ConsoleContext context,
			[MaybeNull] T value)
			=> _Predicate.Invoke(value) ? SuccessResult.Instance.Task : FailureResult.Instance.Task;
	}
}
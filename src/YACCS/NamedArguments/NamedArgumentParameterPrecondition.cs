using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	public class NamedArgumentParameterPrecondition
		: ParameterPrecondition<IContext, IDictionary<string, object?>>
	{
		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;

		protected virtual IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;

		public NamedArgumentParameterPrecondition(Type type)
		{
			_Parameters = new Lazy<IReadOnlyDictionary<string, IImmutableParameter>>(() =>
			{
				return NamedArgumentUtils.CreateParameters(type)
					.ToParameterDictionary(x => x.ParameterName);
			});
		}

		public override async Task<IResult> CheckAsync(
			ParameterInfo parameter,
			IContext context,
			[MaybeNull] IDictionary<string, object?> value)
		{
			foreach (var kvp in value)
			{
				var namedParameter = Parameters[kvp.Key];
				var parameterInfo = new ParameterInfo(parameter.Command, namedParameter);
				foreach (var precondition in namedParameter.Preconditions)
				{
					var result = await precondition.CheckAsync(parameterInfo, context, value).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}
			}
			return SuccessResult.Instance.Sync;
		}
	}
}
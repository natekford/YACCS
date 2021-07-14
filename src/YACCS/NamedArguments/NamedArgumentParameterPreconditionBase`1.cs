using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	public abstract class NamedArgumentParameterPreconditionBase<T>
		: ParameterPrecondition<IContext, T>
	{
		protected abstract IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

		public override async ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			T? value)
		{
			if (value is null)
			{
				return new NullParameterResult(parameter);
			}

			foreach (var kvp in Parameters)
			{
				var (id, member) = kvp;
				var result = await member.Preconditions
					.ProcessAsync(x => x.CheckAsync(command, member, context, Getter(value, id)))
					.ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		protected abstract object? Getter(T instance, string property);
	}
}
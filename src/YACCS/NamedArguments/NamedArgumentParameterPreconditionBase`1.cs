using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

		public override async Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			[MaybeNull] T value)
		{
			foreach (var kvp in Parameters)
			{
				var (id, member) = kvp;
				foreach (var precondition in member.Preconditions)
				{
					var memberValue = Getter(value, id);
					var result = await precondition.CheckAsync(command, member, context, memberValue).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}
			}
			return SuccessResult.Instance.Sync;
		}

		protected abstract object? Getter(T instance, string property);
	}
}
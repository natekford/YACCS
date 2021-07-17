using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	public abstract class NamedArgumentsParameterPreconditionBase<T>
		: ParameterPrecondition<IContext, T>
	{
		protected abstract IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

		public override async ValueTask<IResult> CheckAsync(
			CommandMeta meta,
			IContext context,
			T? value)
		{
			if (value is null)
			{
				return NullParameterResult.Instance;
			}

			foreach (var kvp in Parameters)
			{
				var (id, member) = kvp;
				var result = await member.Preconditions
					.ProcessAsync(x =>
					{
						var newMeta = new CommandMeta(meta.Command, member);
						var retrieved = Getter(value, id);
						return x.CheckAsync(newMeta, context, retrieved);
					})
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
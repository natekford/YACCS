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

			foreach (var (property, paramater) in Parameters)
			{
				if (!TryGetValue(value, property, out var propertyValue))
				{
					return new NamedArgMissingValueResult(property);
				}

				var newMeta = new CommandMeta(meta.Command, paramater);
				var result = await paramater.CanExecuteAsync(newMeta, context, propertyValue).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		protected abstract bool TryGetValue(T instance, string property, out object? value);
	}
}
using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions.Existence
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public abstract class ExistenceParameterPreconditionAttribute
		: ParameterPreconditionAttribute
	{
		protected Item Status { get; set; }

		protected ExistenceParameterPreconditionAttribute(Item status)
		{
			Status = status;
		}

		protected override async ValueTask<IResult> CheckAsync(
			CommandMeta meta,
			IContext context,
			object? value)
		{
			// TODO: once generic attributes are allowed, rewrite this so we don't have to
			// rely on value.GetType().
			// Not going to bother writing null checks/caching/virtual methods until then
			var exists = await DoesExistAsync(meta, context, value).ConfigureAwait(false);
			if (exists && Status == Item.MustNotExist)
			{
				return new ExistenceMustNotExist(value!.GetType());
			}
			else if (!exists && Status == Item.MustExist)
			{
				return new ExistenceMustExist(value!.GetType());
			}
			return SuccessResult.Instance;
		}

		protected abstract ValueTask<bool> DoesExistAsync(
			CommandMeta meta,
			IContext context,
			object? value);
	}
}
using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions.InUse
{
	/// <summary>
	/// The base class for an in use parameter precondition attribute.
	/// </summary>
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public abstract class InUseParameterPreconditionAttribute
		: ParameterPreconditionAttribute
	{
		/// <summary>
		/// The item status to match.
		/// </summary>
		protected Item Status { get; set; }

		/// <summary>
		/// Creates a new <see cref="InUseParameterPreconditionAttribute"/>.
		/// </summary>
		/// <param name="status">
		/// <inheritdoc cref="Status" path="/summary"/>
		/// </param>
		protected InUseParameterPreconditionAttribute(Item status)
		{
			Status = status;
		}

		/// <inheritdoc />
		public override async ValueTask<IResult> CheckAsync(
			CommandMeta meta,
			IContext context,
			object? value)
		{
			// TODO: once generic attributes are allowed, rewrite this so we don't have to
			// rely on value.GetType().
			// Not going to bother writing null checks/caching/virtual methods until then
			var exists = await IsInUseAsync(meta, context, value).ConfigureAwait(false);
			if (exists && Status == Item.MustNotBeInUse)
			{
				return new InUseMustNotBeInUse(value!.GetType());
			}
			else if (!exists && Status == Item.MustBeInUser)
			{
				return new InUseMustBeInUse(value!.GetType());
			}
			return SuccessResult.Instance;
		}

		/// <summary>
		/// Determines if <paramref name="value"/> is already in use.
		/// </summary>
		/// <returns>A bool indicating if the searched for item is in use.</returns>
		/// <inheritdoc cref="CheckAsync(CommandMeta, IContext, object?)"/>
		protected abstract ValueTask<bool> IsInUseAsync(
			CommandMeta meta,
			IContext context,
			object? value);
	}
}
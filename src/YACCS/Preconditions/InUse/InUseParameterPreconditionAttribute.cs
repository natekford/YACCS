using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions.InUse
{
	/// <summary>
	/// The base class for an in use parameter precondition attribute.
	/// </summary>
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public abstract class InUseParameterPrecondition<TContext, TValue>
		: ParameterPrecondition<TContext, TValue>
		where TContext : IContext
	{
		/// <summary>
		/// The item status to match.
		/// </summary>
		protected Item Status { get; set; }

		/// <summary>
		/// Creates a new <see cref="InUseParameterPrecondition{TContext, TValue}"/>.
		/// </summary>
		/// <param name="status">
		/// <inheritdoc cref="Status" path="/summary"/>
		/// </param>
		protected InUseParameterPrecondition(Item status)
		{
			Status = status;
		}

		/// <inheritdoc />
		public override async ValueTask<IResult> CheckAsync(
			CommandMeta meta,
			TContext context,
			TValue? value)
		{
			var exists = await IsInUseAsync(meta, context, value).ConfigureAwait(false);
			if (exists && Status == Item.MustNotBeInUse)
			{
				return new InUseMustNotBeInUse(typeof(TValue));
			}
			else if (!exists && Status == Item.MustBeInUser)
			{
				return new InUseMustBeInUse(typeof(TValue));
			}
			return SuccessResult.Instance;
		}

		/// <summary>
		/// Determines if <paramref name="value"/> is already in use.
		/// </summary>
		/// <returns>A bool indicating if the searched for item is in use.</returns>
		/// <inheritdoc cref="CheckAsync(CommandMeta, TContext, TValue?)"/>
		protected abstract ValueTask<bool> IsInUseAsync(
			CommandMeta meta,
			IContext context,
			object? value);
	}
}
using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Preconditions.Locked;

/// <summary>
/// The base class for a parameter precondition that makes sure the supplied parameter
/// is not currently in use.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public abstract class Locked<TContext, TValue> : ParameterPrecondition<TContext, TValue>
	where TContext : IContext
{
	/// <summary>
	/// The item status to match.
	/// </summary>
	protected Item Status { get; set; }

	/// <summary>
	/// Creates a new <see cref="Locked{TContext, TValue}"/>.
	/// </summary>
	/// <param name="status">
	/// <inheritdoc cref="Status" path="/summary"/>
	/// </param>
	protected Locked(Item status)
	{
		Status = status;
	}

	/// <inheritdoc />
	public override async ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		TContext context,
		TValue? value)
	{
		var locked = await IsLockedAsync(meta, context, value).ConfigureAwait(false);
		if (locked && Status == Item.Unlocked)
		{
			return new MustBeUnlocked(typeof(TValue));
		}
		else if (!locked && Status == Item.Locked)
		{
			return new MustBeLocked(typeof(TValue));
		}
		return Success.Instance;
	}

	/// <summary>
	/// Determines if <paramref name="value"/> is already in use.
	/// </summary>
	/// <returns>A bool indicating if the searched for item is in use.</returns>
	/// <inheritdoc cref="CheckAsync(CommandMeta, TContext, TValue?)"/>
	protected abstract ValueTask<bool> IsLockedAsync(
		CommandMeta meta,
		IContext context,
		object? value);
}
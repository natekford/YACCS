using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.NamedArguments;

/// <summary>
/// The base class for a named arguments parameter precondition.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NamedArgumentsParameterPreconditionBase<T> :
	ParameterPrecondition<IContext, T>
{
	/// <summary>
	/// The parameters this precondition expects.
	/// </summary>
	/// <remarks>
	/// The keys are the original parameter name, NOT the current localized parameter name.
	/// </remarks>
	protected abstract IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

	/// <inheritdoc />
	public override async ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IContext context,
		T? value)
	{
		if (value is null)
		{
			return NullParameterResult.Instance;
		}

		foreach (var (property, parameter) in Parameters)
		{
			if (!TryGetProperty(value, property, out var propertyValue))
			{
				return new NamedArgMissingValueResult(property);
			}

			var result = await meta.Command.CanExecuteAsync(parameter, context, propertyValue).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}
		}
		return SuccessResult.Instance;
	}

	/// <summary>
	/// Tries to get the value of the property with the name <paramref name="property"/>.
	/// </summary>
	/// <param name="instance">The instance to get the value from.</param>
	/// <param name="property">The property to get the value from.</param>
	/// <param name="value">The value of the property.</param>
	/// <returns>A bool indicating success or failure.</returns>
	protected abstract bool TryGetProperty(T instance, string property, out object? value);
}
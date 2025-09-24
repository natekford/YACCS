using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.NamedArguments;

/// <summary>
/// The base class for a named arguments parameter precondition.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NamedArgumentsParameterPreconditionBase<T>
	: ParameterPrecondition<IContext, T>, INamedArgumentParameters
{
	/// <inheritdoc />
	public abstract IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

	/// <inheritdoc />
	protected override async ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IContext context,
		T value)
	{
		foreach (var (property, parameter) in Parameters)
		{
			if (!TryGetProperty(value, property, out var propertyValue))
			{
				return Result.NamedArgMissingValue(property);
			}

			var result = await meta.Command.CanExecuteAsync(parameter, context, propertyValue).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}
		}
		return Result.EmptySuccess;
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
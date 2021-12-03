using System.Collections.Immutable;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments;

internal static class NamedArgumentsUtils
{
	internal static ImmutableDictionary<string, IImmutableParameter> CreateParamDict(
		this Type type,
		Func<IImmutableParameter, string> keySelector)
	{
		var (properties, fields) = type.GetWritableMembers();
		return properties
			.Select(x => new Parameter(x))
			.Concat(fields.Select(x => new Parameter(x)))
			.Select(x => x.ToImmutable())
			.ToParamDict(keySelector);
	}

	internal static ImmutableDictionary<string, IImmutableParameter> ToParamDict(
		this IEnumerable<IImmutableParameter> parameters,
		Func<IImmutableParameter, string> keySelector)
		=> parameters.ToImmutableDictionary(keySelector, StringComparer.OrdinalIgnoreCase);
}
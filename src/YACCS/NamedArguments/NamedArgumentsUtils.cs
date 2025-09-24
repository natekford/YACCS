using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments;

internal static class NamedArgumentsUtils
{
	internal static FrozenDictionary<string, IImmutableParameter> CreateParamDict(
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

	internal static FrozenDictionary<string, IImmutableParameter> ToParamDict(
		this IEnumerable<IImmutableParameter> parameters,
		Func<IImmutableParameter, string> keySelector)
		=> parameters.ToFrozenDictionary(keySelector, StringComparer.OrdinalIgnoreCase);
}
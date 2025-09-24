using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments;

internal static class NamedArgumentsUtils
{
	internal static IReadOnlyList<IImmutableParameter> CreateParameters(this Type type)
	{
		var (properties, fields) = type.GetWritableMembers();
		return [.. properties
			.Select(x => new Parameter(x))
			.Concat(fields.Select(x => new Parameter(x)))
			.Select(x => x.ToImmutable())
		];
	}

	internal static IImmutableParameter? GetParameter(
		this IEnumerable<IImmutableParameter> parameters,
		string search)
	{
		var pName = default(IImmutableParameter?);
		var oName = default(IImmutableParameter?);
		foreach (var parameter in parameters)
		{
			if (parameter.ParameterName?.Name is string n
				&& n.Equals(search, StringComparison.OrdinalIgnoreCase))
			{
				pName = parameter;
			}
			else if (parameter.OriginalParameterName.Equals(search, StringComparison.OrdinalIgnoreCase))
			{
				oName = parameter;
			}
		}
		return pName ?? oName;
	}
}
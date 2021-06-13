using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments
{
	public static class NamedArgumentUtils
	{
		public static IEnumerable<IImmutableParameter> CreateParametersForType(Type type)
		{
			var (properties, fields) = type.GetWritableMembers();
			return properties
				.Select(x => new Parameter(x))
				.Concat(fields.Select(x => new Parameter(x)))
				.Select(x => x.ToImmutable(null));
		}
	}
}
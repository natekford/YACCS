using System;
using System.Collections.Generic;

using YACCS.ParameterPreconditions;

namespace YACCS.Commands.Models
{
	public interface IImmutableParameter : IImmutableEntityBase
	{
		Type? EnumerableType { get; }
		bool IsOptional { get; }
		int Length { get; }
		string ParameterName { get; }
		Type ParameterType { get; }
		IReadOnlyList<IParameterPrecondition> Preconditions { get; }
	}
}
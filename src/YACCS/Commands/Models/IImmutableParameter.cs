using System;
using System.Collections.Generic;

using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	public interface IImmutableParameter : IImmutableEntityBase
	{
		object? DefaultValue { get; }
		Type? EnumerableType { get; }
		int Length { get; }
		ITypeReader? OverriddenTypeReader { get; }
		string ParameterName { get; }
		Type ParameterType { get; }
		IReadOnlyList<IParameterPrecondition> Preconditions { get; }
	}
}
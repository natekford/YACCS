using System;
using System.Collections.Generic;

using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	public interface IImmutableParameter : IImmutableEntityBase, IQueryableParameter
	{
		object? DefaultValue { get; }
		Type? EnumerableType { get; }
		bool HasDefaultValue { get; }
		int? Length { get; }
		string OverriddenParameterName { get; }
		ITypeReader? OverriddenTypeReader { get; }
		IReadOnlyList<IParameterPrecondition> Preconditions { get; }
	}
}
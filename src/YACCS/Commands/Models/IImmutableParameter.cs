using System.Collections.Generic;

using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	public interface IImmutableParameter : IImmutableEntityBase, IQueryableParameter
	{
		object? DefaultValue { get; }
		bool HasDefaultValue { get; }
		int? Length { get; }
		string ParameterName { get; }
		IReadOnlyDictionary<string, IReadOnlyList<IParameterPrecondition>> Preconditions { get; }
		ITypeReader? TypeReader { get; }
	}
}
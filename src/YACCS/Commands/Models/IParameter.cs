using System;

using YACCS.TypeReaders;

namespace YACCS.Commands.Models
{
	public interface IParameter : IEntityBase
	{
		object? DefaultValue { get; set; }
		bool HasDefaultValue { get; set; }
		ITypeReader? OverriddenTypeReader { get; set; }
		string ParameterName { get; set; }
		Type ParameterType { get; set; }

		IImmutableParameter ToParameter();
	}
}
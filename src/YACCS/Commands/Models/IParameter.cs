using System;

namespace YACCS.Commands.Models
{
	public interface IParameter : IEntityBase
	{
		object? DefaultValue { get; set; }
		string ParameterName { get; set; }
		Type ParameterType { get; set; }

		IImmutableParameter ToParameter();
	}
}
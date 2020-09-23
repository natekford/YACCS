using System;

namespace YACCS.Commands.Models
{
	public interface IParameter : IEntityBase
	{
		bool IsOptional { get; set; }
		string ParameterName { get; set; }
		Type ParameterType { get; set; }

		IImmutableParameter ToParameter();
	}
}
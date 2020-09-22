using System;

namespace YACCS.Commands.Models
{
	public interface IMutableParameter : IMutableEntityBase
	{
		bool IsOptional { get; set; }
		string ParameterName { get; set; }
		Type ParameterType { get; set; }

		IParameter ToParameter();
	}
}
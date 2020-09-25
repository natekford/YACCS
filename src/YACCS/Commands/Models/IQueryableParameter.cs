using System;

namespace YACCS.Commands.Models
{
	public interface IQueryableParameter
	{
		string ParameterName { get; }
		Type ParameterType { get; }
	}
}
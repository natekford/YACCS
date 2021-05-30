using System;

namespace YACCS.Commands.Models
{
	public interface IQueryableParameter
	{
		string OriginalParameterName { get; }
		Type ParameterType { get; }
	}
}
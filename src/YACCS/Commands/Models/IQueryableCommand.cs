using System;
using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IQueryableCommand : IQueryableEntity
	{
		Type ContextType { get; }
		IEnumerable<IReadOnlyList<string>> Names { get; }
		IReadOnlyList<IQueryableParameter> Parameters { get; }
	}
}
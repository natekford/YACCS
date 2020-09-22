using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public interface ICommand : IEntityBase, IQueryableCommand, IContextValidator
	{
		new IReadOnlyList<IName> Names { get; }
		IReadOnlyList<IParameter> Parameters { get; }
		IReadOnlyList<IPrecondition> Preconditions { get; }
		int Priority { get; }

		Task<ExecutionResult> GetResultAsync(IContext context, object?[] args);
	}
}
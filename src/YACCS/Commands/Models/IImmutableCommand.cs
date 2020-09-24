using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public interface IImmutableCommand : IImmutableEntityBase, IQueryableCommand, IContextValidator
	{
		new IReadOnlyList<IName> Names { get; }
		IReadOnlyList<IImmutableParameter> Parameters { get; }
		IReadOnlyList<IPrecondition> Preconditions { get; }
		int Priority { get; }

		Task<ExecutionResult> ExecuteAsync(IContext context, object?[] args);
	}
}
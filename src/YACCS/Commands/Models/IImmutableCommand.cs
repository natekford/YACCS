using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public interface IImmutableCommand : IImmutableEntityBase, IQueryableCommand
	{
		int MaxLength { get; }
		int MinLength { get; }
		new IReadOnlyList<IReadOnlyList<string>> Names { get; }
		IReadOnlyList<IImmutableParameter> Parameters { get; }
		IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions { get; }
		int Priority { get; }

		Task<IResult> ExecuteAsync(IContext context, object?[] args);
	}
}
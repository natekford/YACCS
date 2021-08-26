using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public interface IImmutableCommand : IImmutableEntityBase, IQueryableCommand
	{
		bool IsHidden { get; }
		int MaxLength { get; }
		int MinLength { get; }
		new IReadOnlyList<IReadOnlyList<string>> Names { get; }
		new IReadOnlyList<IImmutableParameter> Parameters { get; }
		IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions { get; }
		int Priority { get; }
		IImmutableCommand? Source { get; }

		ValueTask<IResult> ExecuteAsync(IContext context, object?[] args);
	}
}
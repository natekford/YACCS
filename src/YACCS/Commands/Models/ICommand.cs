using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface ICommand : IEntityBase, IQueryableCommand
	{
		new IList<IReadOnlyList<string>> Names { get; set; }
		IList<IParameter> Parameters { get; set; }

		IImmutableCommand MakeImmutable();

		IEnumerable<IImmutableCommand> MakeMultipleImmutable();
	}
}
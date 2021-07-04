using System.Collections.Generic;

using YACCS.Commands.Attributes;

namespace YACCS.Commands.Models
{
	public interface ICommand : IEntityBase, IQueryableCommand
	{
		new IList<IReadOnlyList<string>> Names { get; set; }
		new IReadOnlyList<IParameter> Parameters { get; set; }

		IImmutableCommand MakeImmutable();

		IEnumerable<IImmutableCommand> MakeMultipleImmutable();
	}
}
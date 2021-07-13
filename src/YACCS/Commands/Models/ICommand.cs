using System;
using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface ICommand : IEntityBase, IQueryableCommand
	{
		new IList<IReadOnlyList<string>> Names { get; set; }
		new IReadOnlyList<IParameter> Parameters { get; set; }

		IImmutableCommand ToImmutable();

		IAsyncEnumerable<IImmutableCommand> ToMultipleImmutableAsync(IServiceProvider services);
	}
}
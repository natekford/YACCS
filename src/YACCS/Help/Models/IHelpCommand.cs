using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models
{
	/// <summary>
	/// Information to display about a command in a help command.
	/// </summary>
	public interface IHelpCommand : IHelpItem<IImmutableCommand>
	{
		/// <summary>
		/// The context type of this command.
		/// </summary>
		IHelpItem<Type> ContextType { get; }
		/// <summary>
		/// The parameters of this command.
		/// </summary>
		IReadOnlyList<IHelpParameter> Parameters { get; }
		/// <summary>
		/// The precondition groups of this command.
		/// </summary>
		IReadOnlyDictionary<string, ILookup<BoolOp, IHelpItem<IPrecondition>>> Preconditions { get; }
	}
}
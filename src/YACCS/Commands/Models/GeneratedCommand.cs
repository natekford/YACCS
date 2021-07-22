using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public abstract class GeneratedCommand : IImmutableCommand
	{
		public IImmutableCommand Source { get; }
		public virtual IReadOnlyList<object> Attributes => Source.Attributes;
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		public virtual Type ContextType => Source.ContextType;
		public virtual int MaxLength => Source.MaxLength;
		public virtual int MinLength => Source.MinLength;
		public virtual IReadOnlyList<IReadOnlyList<string>> Names => Source.Names;
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
		public virtual IReadOnlyList<IImmutableParameter> Parameters => Source.Parameters;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
		public virtual IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions => Source.Preconditions;
		public virtual string PrimaryId => Source.PrimaryId;
		public virtual int Priority => Source.Priority;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		protected GeneratedCommand(IImmutableCommand source)
		{
			Source = source;
		}

		public abstract ValueTask<IResult> ExecuteAsync(IContext context, object?[] args);
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	/// <summary>
	/// A command generated from another command.
	/// </summary>
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public abstract class GeneratedCommand : IImmutableCommand
	{
		/// <inheritdoc />
		public IImmutableCommand Source { get; }
		/// <inheritdoc />
		public virtual IReadOnlyList<object> Attributes => Source.Attributes;
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		/// <inheritdoc />
		public virtual Type ContextType => Source.ContextType;
		/// <inheritdoc />
		public virtual bool IsHidden => Source.IsHidden;
		/// <inheritdoc />
		public virtual int MaxLength => Source.MaxLength;
		/// <inheritdoc />
		public virtual int MinLength => Source.MinLength;
		/// <inheritdoc />
		public virtual IReadOnlyList<IReadOnlyList<string>> Names => Source.Names;
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
		/// <inheritdoc />
		public virtual IReadOnlyList<IImmutableParameter> Parameters => Source.Parameters;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
		/// <inheritdoc />
		public virtual IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions => Source.Preconditions;
		/// <inheritdoc />
		public virtual string PrimaryId => Source.PrimaryId;
		/// <inheritdoc />
		public virtual int Priority => Source.Priority;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		/// <summary>
		/// Creates a new <see cref="GeneratedCommand"/> and sets <see cref="Source"/>
		/// to <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		protected GeneratedCommand(IImmutableCommand source)
		{
			Source = source;
		}

		/// <inheritdoc />
		public abstract ValueTask<IResult> ExecuteAsync(IContext context, object?[] args);

		/// <inheritdoc />
		public virtual bool IsValidContext(Type type)
			=> Source.IsValidContext(type);
	}
}
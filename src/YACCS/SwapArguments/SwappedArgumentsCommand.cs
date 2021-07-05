using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.SwapArguments
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class SwappedArgumentsCommand : IImmutableCommand
	{
		private readonly Swapper _Swapper;

		public IReadOnlyList<object> Attributes { get; }
		public IReadOnlyList<IImmutableParameter> Parameters { get; }
		public int Priority { get; }
		public IImmutableCommand Source { get; }
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		public Type? ContextType => Source.ContextType;
		public int MaxLength => Source.MaxLength;
		public int MinLength => Source.MinLength;
		public IReadOnlyList<IReadOnlyList<string>> Names => Source.Names;
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
		public IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions => Source.Preconditions;
		public string PrimaryId => Source.PrimaryId;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		public SwappedArgumentsCommand(
			IImmutableCommand source,
			Swapper swapper,
			int priorityDifference)
		{
			_Swapper = swapper;
			Source = source;
			Priority = source.Priority + (priorityDifference * _Swapper.Swaps.Length);
			Attributes = source.CreateGeneratedCommandAttributeList();

			var builder = ImmutableArray.CreateBuilder<IImmutableParameter>(Source.Parameters.Count);
			builder.AddRange(source.Parameters);
			_Swapper.Swap(builder);
			Parameters = builder.MoveToImmutable();
		}

		public Task<IResult> ExecuteAsync(IContext context, object?[] args)
		{
			var copy = args.ToArray();
			_Swapper.SwapBack(copy);
			return Source.ExecuteAsync(context, copy);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.SwapArguments
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class SwappedArgumentsCommand : IImmutableCommand
	{
		private readonly ImmutableArray<int> _SwapIndices;

		public IReadOnlyList<IImmutableParameter> Parameters { get; }
		public int Priority { get; }
		public IImmutableCommand Source { get; }
		public IReadOnlyList<object> Attributes => Source.Attributes;
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		public Type? ContextType => Source.ContextType;
		public int MaxLength => Source.MaxLength;
		public int MinLength => Source.MinLength;
		public IReadOnlyList<IReadOnlyList<string>> Names => Source.Names;
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
		public IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions => Source.Preconditions;
		public string PrimaryId => Source.PrimaryId;
		private string DebuggerDisplay => $"Name = {Names?.FirstOrDefault()?.ToString() ?? "NULL"}, Parameter Count = {Parameters.Count}";

		public SwappedArgumentsCommand(
			IImmutableCommand source,
			ImmutableArray<int> swapIndices,
			int priorityDifference)
		{
			Source = source;
			_SwapIndices = swapIndices;
			Priority = source.Priority + (priorityDifference * (_SwapIndices.Length - 1));

			var builder = ImmutableArray.CreateBuilder<IImmutableParameter>(Source.Parameters.Count);
			builder.AddRange(source.Parameters);
			Map(builder);
			Parameters = builder.MoveToImmutable();
		}

		public Task<IResult> ExecuteAsync(IContext context, object?[] args)
		{
			var copy = args.ToArray();
			Unmap(copy);
			return Source.ExecuteAsync(context, copy);
		}

		private void Map<T>(IList<T> source)
		{
			for (var i = 1; i < _SwapIndices.Length; ++i)
			{
				Swap(source, i);
			}
		}

		private void Swap<T>(IList<T> source, int i)
		{
			var temp = source[_SwapIndices[0]];
			source[_SwapIndices[0]] = source[_SwapIndices[i]];
			source[_SwapIndices[i]] = temp;
		}

		private void Unmap<T>(IList<T> source)
		{
			for (var i = _SwapIndices.Length - 1; i >= 1; --i)
			{
				Swap(source, i);
			}
		}
	}
}
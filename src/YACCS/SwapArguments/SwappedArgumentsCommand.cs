﻿using System;
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
		private readonly Swapper _Swapper;

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
			Swapper swapper,
			int priorityDifference)
		{
			_Swapper = swapper;
			Source = source;
			Priority = source.Priority + (priorityDifference * _Swapper.Swaps.Length);

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
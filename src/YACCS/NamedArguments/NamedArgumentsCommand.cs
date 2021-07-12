using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class NamedArgumentsCommand : IImmutableCommand
	{
		private const string CONTEXT_ID = "context_id";
		private const string VALUES_ID = "values_id";

		private readonly IImmutableCommand _Command;

		public IReadOnlyList<object> Attributes { get; }
		public IImmutableCommand Source { get; }
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		public Type ContextType => Source.ContextType;
		public int MaxLength => _Command.MaxLength;
		public int MinLength => _Command.MinLength;
		public IReadOnlyList<IReadOnlyList<string>> Names => Source.Names;
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
		public IReadOnlyList<IImmutableParameter> Parameters => _Command.Parameters;
		public IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions => Source.Preconditions;
		public string PrimaryId => Source.PrimaryId;
		public int Priority => Source.Priority;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		public NamedArgumentsCommand(IImmutableCommand source)
		{
			Source = source;
			Attributes = source.CreateGeneratedCommandAttributeList();

			var @delegate = (Func<IContext, IDictionary<string, object?>, Task<IResult>>)ExecuteAsync;
			var command = new DelegateCommand(@delegate, this);
			var parameter = command.Parameters.ById(VALUES_ID).Single();
			parameter
				.AsType<IDictionary<string, object?>>()
				.AddParameterPrecondition(new GeneratedNamedParameterPrecondition(Source))
				.SetTypeReader(new GeneratedNamedTypeReader(Source));

			_Command = command.MakeImmutable();
		}

		public Task<IResult> ExecuteAsync(IContext context, object?[] args)
			=> _Command.ExecuteAsync(context, args);

		private Task<IResult> ExecuteAsync(
			[Id(CONTEXT_ID)]
			[Context]
			IContext context,
			[Id(VALUES_ID)]
			[Remainder]
			IDictionary<string, object?> values)
		{
			var args = new object?[Source.Parameters.Count];
			for (var i = 0; i < args.Length; ++i)
			{
				var parameter = Source.Parameters[i];
				if (values.TryGetValue(parameter.OriginalParameterName, out var value))
				{
					args[i] = value;
				}
				else if (parameter.HasDefaultValue)
				{
					args[i] = parameter.DefaultValue;
				}
				else
				{
					// This should never really be reachable due to GeneratedNamedParameterPrecondition
					// already checking for undefined values
					throw new InvalidOperationException(
						$"Missing value for the parameter '{parameter.OriginalParameterName}' " +
						$"from '{Source.Names?.FirstOrDefault()}'.");
				}
			}
			return Source.ExecuteAsync(context, args);
		}

		private class GeneratedNamedParameterPrecondition
			: NamedArgumentParameterPreconditionBase<Dictionary<string, object?>>
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

			public GeneratedNamedParameterPrecondition(IImmutableCommand command)
			{
				Parameters = command.Parameters
					.ToDictionary(x => x.OriginalParameterName, StringComparer.OrdinalIgnoreCase);
			}

			public override ValueTask<IResult> CheckAsync(
				IImmutableCommand command,
				IImmutableParameter parameter,
				IContext context,
				[MaybeNull] Dictionary<string, object?> value)
			{
				foreach (var kvp in Parameters)
				{
					if (!value.ContainsKey(kvp.Key) && !kvp.Value.HasDefaultValue)
					{
						return new(new NamedArgMissingValueResult(kvp.Key));
					}
				}
				return base.CheckAsync(command, parameter, context, value);
			}

			protected override object? Getter(
				Dictionary<string, object?> instance,
				string property)
				=> instance[property];
		}

		private class GeneratedNamedTypeReader
			: NamedArgumentTypeReaderBase<Dictionary<string, object?>>
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

			public GeneratedNamedTypeReader(IImmutableCommand command)
			{
				Parameters = command.Parameters
					.ToDictionary(x => x.ParameterName, StringComparer.OrdinalIgnoreCase);
			}

			protected override void Setter(
				Dictionary<string, object?> instance,
				string property,
				object? value)
				=> instance[property] = value;
		}
	}
}
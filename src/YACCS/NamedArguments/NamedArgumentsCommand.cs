using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

using ParameterFactory = YACCS.Commands.Linq.Parameters;

namespace YACCS.NamedArguments
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class NamedArgumentsCommand : IImmutableCommand
	{
		public IReadOnlyList<object> Attributes { get; }
		public int MaxLength => int.MaxValue;
		public int MinLength => 0;
		public IReadOnlyList<IImmutableParameter> Parameters { get; }
		public IImmutableCommand Source { get; }
		IEnumerable<object> IQueryableEntity.Attributes => Attributes;
		public Type ContextType => Source.ContextType;
		public IReadOnlyList<IReadOnlyList<string>> Names => Source.Names;
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
		public IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions => Source.Preconditions;
		public string PrimaryId => Source.PrimaryId;
		public int Priority => Source.Priority;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		public NamedArgumentsCommand(IImmutableCommand source)
		{
			Source = source;
			Attributes = source.CreateGeneratedCommandAttributeList();

			var parameters = ImmutableArray.CreateBuilder<IImmutableParameter>(1);
			try
			{
				var parameter = ParameterFactory.Create<IDictionary<string, object?>>("NamedArgDictionary")
					.AddParameterPrecondition(new GeneratedNamedParameterPrecondition(Source))
					.SetTypeReader(new GeneratedNamedTypeReader(Source))
					.AddAttribute(new RemainderAttribute())
					.ToImmutable();
				parameters.Add(parameter);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Unable to build named arguments " +
					$"dictionary parameter for '{Source.Names?.FirstOrDefault()}'.", e);
			}
			Parameters = parameters.MoveToImmutable();
		}

		public ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
		{
			IDictionary<string, object?> values;
			try
			{
				values = (IDictionary<string, object?>)args.Single()!;
			}
			catch (Exception e)
			{
				throw new ArgumentException("Expected named argument dictionary and no " +
					$"other arguments for '{Source.Names?.FirstOrDefault()}'.", e);
			}

			var realArgs = new object?[Source.Parameters.Count];
			for (var i = 0; i < realArgs.Length; ++i)
			{
				var parameter = Source.Parameters[i];
				if (values.TryGetValue(parameter.OriginalParameterName, out var value))
				{
					realArgs[i] = value;
				}
				else if (parameter.HasDefaultValue)
				{
					realArgs[i] = parameter.DefaultValue;
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
			return Source.ExecuteAsync(context, realArgs);
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
				Dictionary<string, object?>? value)
			{
				if (value is null)
				{
					return new(NullParameterResult.Instance);
				}

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
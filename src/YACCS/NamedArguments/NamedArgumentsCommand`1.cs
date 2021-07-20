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

namespace YACCS.NamedArguments
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class NamedArgumentsCommand<TDict> : IImmutableCommand
		where TDict : IDictionary<string, object?>, new()
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
				var parameter = Commands.Linq.Parameters
					.Create<TDict>("NamedArgDictionary")
					.AddParameterPrecondition(new GeneratedNamedArgumentsParameterPrecondition(Source))
					.SetTypeReader(new GeneratedNamedArgumentsTypeReader(Source))
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
			TDict values;
			try
			{
				values = (TDict)args.Single()!;
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
					// This should never really be reachable due to the parameter precondition
					// already checking for undefined values
					throw new InvalidOperationException(
						$"Missing value for the parameter '{parameter.OriginalParameterName}' " +
						$"from '{Source.Names?.FirstOrDefault()}'.");
				}
			}
			return Source.ExecuteAsync(context, realArgs);
		}

		private class GeneratedNamedArgumentsParameterPrecondition
			: NamedArgumentsParameterPreconditionBase<TDict>
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

			public GeneratedNamedArgumentsParameterPrecondition(IImmutableCommand command)
			{
				Parameters = command.Parameters
					.ToImmutableDictionary(x => x.OriginalParameterName, StringComparer.OrdinalIgnoreCase);
			}

			public override ValueTask<IResult> CheckAsync(
				CommandMeta meta,
				IContext context,
				TDict? value)
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
				return base.CheckAsync(meta, context, value);
			}

			protected override bool TryGetValue(TDict instance, string property, out object? value)
				=> instance.TryGetValue(property, out value);
		}

		private class GeneratedNamedArgumentsTypeReader : NamedArgumentsTypeReaderBase<TDict>
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

			public GeneratedNamedArgumentsTypeReader(IImmutableCommand command)
			{
				Parameters = command.Parameters
					.ToImmutableDictionary(x => x.ParameterName, StringComparer.OrdinalIgnoreCase);
			}

			protected override void Setter(TDict instance, string property, object? value)
				=> instance[property] = value;
		}
	}
}
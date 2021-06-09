using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	public static class NamedArgumentUtils
	{
		public static IEnumerable<IImmutableParameter> CreateParametersForType(Type type)
		{
			var (properties, fields) = type.GetWritableMembers();
			return properties
				.Select(x => new Parameter(x))
				.Concat(fields.Select(x => new Parameter(x)))
				.Select(x => x.ToImmutable(null));
		}

		public static IImmutableCommand GenerateNamedArgumentVersion(this IImmutableCommand command)
		{
			const string CONTEXT_ID = "context_id";
			const string VALUES_ID = "values_id";

			Task<IResult> ExecuteAsync(
				[Id(CONTEXT_ID)]
				IContext context,
				[Id(VALUES_ID)]
				IDictionary<string, object?> values)
			{
				var args = new object?[command.Parameters.Count];
				for (var i = 0; i < args.Length; ++i)
				{
					var parameter = command.Parameters[i];
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
						throw new InvalidOperationException("Generated named argument commands cannot handle missing values.");
					}
				}
				return command.ExecuteAsync(context, args);
			}

			var @delegate = (Func<IContext, IDictionary<string, object?>, Task<IResult>>)ExecuteAsync;
			var newCommand = new DelegateCommand(@delegate, command.ContextType, command.Names);
			newCommand.AddAttribute(new GeneratedNamedArgumentsAttribute(command));

			var context = newCommand
				.Parameters
				.ById(CONTEXT_ID)
				.Single()
				.AsType<IContext>();
			context.AddAttribute(new ContextAttribute());

			var values = newCommand
				.Parameters
				.ById(VALUES_ID)
				.Single()
				.AsType<IDictionary<string, object?>>();
			values.AddAttribute(new RemainderAttribute())
				.AddParameterPrecondition(new GeneratedNamedParameterPrecondition(command))
				.SetTypeReader(new GeneratedNamedTypeReader(command));

			return newCommand.ToImmutable().Single();
		}

		private class GeneratedNamedParameterPrecondition
			: NamedArgumentParameterPrecondition<Dictionary<string, object?>>
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

			public GeneratedNamedParameterPrecondition(IImmutableCommand command)
			{
				Parameters = command.Parameters
					.ToDictionary(x => x.OriginalParameterName, StringComparer.OrdinalIgnoreCase);
			}

			public override Task<IResult> CheckAsync(
				IImmutableParameter parameter,
				IContext context,
				[MaybeNull] Dictionary<string, object?> value)
			{
				foreach (var kvp in Parameters)
				{
					if (!value.ContainsKey(kvp.Key) && !kvp.Value.HasDefaultValue)
					{
						return new NamedArgMissingValueResult(kvp.Key).AsTask();
					}
				}
				return base.CheckAsync(parameter, context, value);
			}

			protected override object? Getter(
				Dictionary<string, object?> instance,
				string property)
				=> instance[property];
		}

		private class GeneratedNamedTypeReader
			: NamedArgumentTypeReader<Dictionary<string, object?>>
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
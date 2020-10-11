﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Parsing;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	public static class NamedArgumentUtils
	{
		public static IEnumerable<IImmutableParameter> CreateParameters<T>()
			=> CreateParameters(typeof(T));

		public static IEnumerable<IImmutableParameter> CreateParameters(Type type)
		{
			var (properties, fields) = type.GetWritableMembers();
			return properties
				.Select(x => new Parameter(x))
				.Concat(fields.Select(x => new Parameter(x)))
				.Select(x => x.ToImmutable());
		}

		public static IImmutableCommand GenerateNamedArgumentVersion(this IImmutableCommand command)
		{
			const string CONTEXT_ID = "context_id";
			const string VALUES_ID = "values_id";

			Task<ExecutionResult> ExecuteAsync(
				[Id(CONTEXT_ID)]
				IContext context,
				[Id(VALUES_ID)]
				IDictionary<string, object?> values)
			{
				var args = new object?[command.Parameters.Count];
				for (var i = 0; i < args.Length; ++i)
				{
					var parameter = command.Parameters[i];
					if (values.TryGetValue(parameter.ParameterName, out var value))
					{
						args[i] = value;
					}
					else if (parameter.HasDefaultValue)
					{
						args[i] = parameter.DefaultValue;
					}
					else
					{
						throw new InvalidOperationException("Generated named argument commands cannot handle missing values.");
					}
				}
				return command.ExecuteAsync(context, args);
			}

			var @delegate = (Func<IContext, IDictionary<string, object?>, Task<ExecutionResult>>)ExecuteAsync;
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
				.AddParameterPrecondition(new GeneratedNamedParameterPrecondition(command, values.ParameterType))
				.SetOverriddenTypeReader(new GeneratedNamedTypeReader(command));

			return newCommand.ToImmutable().Single();
		}

		public static IReadOnlyDictionary<string, IImmutableParameter> ToParameterDictionary(
			this IEnumerable<IImmutableParameter> parameters,
			Func<IImmutableParameter, string> keySelector)
		{
			return parameters
				.ToDictionary(keySelector, x => x, StringComparer.OrdinalIgnoreCase);
		}

		private class GeneratedNamedParameterPrecondition : NamedArgumentParameterPrecondition
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

			public GeneratedNamedParameterPrecondition(IImmutableCommand command, Type type)
				: base(type)
			{
				Parameters = command.Parameters.ToParameterDictionary(x => x.ParameterName);
			}
		}

		private class GeneratedNamedTypeReader : NamedArgumentTypeReader<Dictionary<string, object?>>
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }
			protected override Action<Dictionary<string, object?>, string, object> Setter { get; }

			public GeneratedNamedTypeReader(IImmutableCommand command)
			{
				Setter = (dict, key, value) => dict[key] = value;
				Parameters = command.Parameters.ToParameterDictionary(x => x.OverriddenParameterName);
			}

			protected override bool TryCreateDict(ParseArgs args, [NotNullWhen(true)] out IDictionary<string, string>? dict)
			{
				if (!base.TryCreateDict(args, out dict))
				{
					return false;
				}

				foreach (var kvp in Parameters)
				{
					if (!dict.ContainsKey(kvp.Key) && !kvp.Value.HasDefaultValue)
					{
						return false;
					}
				}
				return true;
			}
		}
	}
}
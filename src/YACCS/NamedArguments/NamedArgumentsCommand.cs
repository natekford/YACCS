﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;

using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace YACCS.NamedArguments;

/// <inheritdoc />
public sealed class NamedArgumentsCommand : GeneratedCommand
{
	/// <inheritdoc />
	public override int MaxLength => int.MaxValue;
	/// <inheritdoc />
	public override int MinLength => 0;
	/// <inheritdoc />
	public override IReadOnlyList<IImmutableParameter> Parameters { get; }

	/// <summary>
	/// Creates a new <see cref="NamedArgumentsCommand"/>.
	/// </summary>
	/// <inheritdoc cref="GeneratedCommand(IImmutableCommand, int)"/>
	public NamedArgumentsCommand(IImmutableCommand source, int priorityDifference)
		: base(source, priorityDifference)
	{
		try
		{
			var parameter = Commands.Linq.Parameters.Create<Dict>("NamedArgDictionary")
				.AddParameterPrecondition(new GeneratedNamedArgumentsParameterPrecondition(Source))
				.SetTypeReader(new GeneratedNamedArgumentsTypeReader(Source))
				.AddAttribute(new RemainderAttribute())
				.ToImmutable();
			Parameters = [parameter];
		}
		catch (Exception e)
		{
			throw new InvalidOperationException($"Unable to build {typeof(Dict).Name} " +
				$"parameter for '{source.Paths?.FirstOrDefault()}'.", e);
		}
	}

	/// <inheritdoc />
	public override ValueTask<IResult> ExecuteAsync(
		IContext context,
		IReadOnlyList<object?> args)
	{
		if (args.Count != 1 || args[0] is not Dict dict)
		{
			return new(Result.NamedArgInvalidDictionary);
		}

		var values = new object?[Source.Parameters.Count];
		for (var i = 0; i < values.Length; ++i)
		{
			// There shouldn't be any KNFExceptions because the type readers/preconditions
			// are already setting default values and checking for undefined values
			values[i] = dict[Source.Parameters[i].OriginalParameterName];
		}
		return Source.ExecuteAsync(context, values);
	}

	private sealed class GeneratedNamedArgumentsParameterPrecondition(IImmutableCommand command)
		: NamedArgumentsParameterPreconditionBase<Dict>
	{
		public override IReadOnlyList<IImmutableParameter> Parameters { get; }
			= command.Parameters;

		protected override bool TryGetProperty(Dict instance, string property, out object? value)
		{
			// If the value is already in the dictionary, we use that
			// If it's not, check if it has a default value
			// No value or default value? Indicate failure
			if (!instance.TryGetValue(property, out value))
			{
				var parameter = Parameters.GetParameter(property)!;
				if (!parameter.HasDefaultValue)
				{
					return false;
				}
				value = instance[property] = parameter.DefaultValue;
			}
			return true;
		}
	}

	private sealed class GeneratedNamedArgumentsTypeReader(IImmutableCommand command)
		: NamedArgumentsTypeReaderBase<Dict>
	{
		public override IReadOnlyList<IImmutableParameter> Parameters { get; }
			= command.Parameters;

		protected override void SetProperty(Dict instance, string property, object? value)
			=> instance[property] = value;
	}
}
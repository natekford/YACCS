using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	public class NamedArgumentsCommand<T> : GeneratedCommand
		where T : IDictionary<string, object?>, new()
	{
		public override int MaxLength => int.MaxValue;
		public override int MinLength => 0;
		public override IReadOnlyList<IImmutableParameter> Parameters { get; }

		public NamedArgumentsCommand(IImmutableCommand source) : base(source)
		{
			var parameters = ImmutableArray.CreateBuilder<IImmutableParameter>(1);
			try
			{
				var parameter = Commands.Linq.Parameters
					.Create<T>("NamedArgDictionary")
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

		public override ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
		{
			T dict;
			try
			{
				dict = (T)args.Single()!;
			}
			catch (Exception e)
			{
				throw new ArgumentException("Expected named argument dictionary and no " +
					$"other arguments for '{Source.Names?.FirstOrDefault()}'.", e);
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

		private class GeneratedNamedArgumentsParameterPrecondition
			: NamedArgumentsParameterPreconditionBase<T>
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

			public GeneratedNamedArgumentsParameterPrecondition(IImmutableCommand command)
			{
				Parameters = command.Parameters.ToParamDict(x => x.OriginalParameterName);
			}

			protected override bool TryGetProperty(T instance, string property, out object? value)
			{
				// If the value is already in the dictionary, we use that
				// If it's not, check if it has a default value
				// No value or default value? Indicate failure
				if (!instance.TryGetValue(property, out value))
				{
					var parameter = Parameters[property];
					if (!parameter.HasDefaultValue)
					{
						return false;
					}
					value = instance[property] = parameter.DefaultValue;
				}
				return true;
			}
		}

		private class GeneratedNamedArgumentsTypeReader : NamedArgumentsTypeReaderBase<T>
		{
			protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }

			public GeneratedNamedArgumentsTypeReader(IImmutableCommand command)
			{
				Parameters = command.Parameters.ToParamDict(x => x.ParameterName);
			}

			protected override void SetProperty(T instance, string property, object? value)
				=> instance[property] = value;
		}
	}
}
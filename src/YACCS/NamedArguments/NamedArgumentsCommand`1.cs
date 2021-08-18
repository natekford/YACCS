using System.Collections.Generic;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.StructuredArguments;

namespace YACCS.NamedArguments
{
	public class NamedArgumentsCommand<T> : StructuredArgumentsCommand<T>
		where T : IDictionary<string, object?>, new()
	{
		public override int MaxLength => int.MaxValue;
		public override int MinLength => 0;
		public override IReadOnlyList<IImmutableParameter> Parameters { get; }

		public NamedArgumentsCommand(IImmutableCommand source) : base(source)
		{
			Parameters = Source.CreateStructuredParameter<T>("NamedArgDictionary", x =>
			{
				x.AddParameterPrecondition(new GeneratedNamedArgumentsParameterPrecondition(Source))
				.SetTypeReader(new GeneratedNamedArgumentsTypeReader(Source))
				.AddAttribute(new RemainderAttribute());
			});
		}

		protected override object? GetValue(T structured, IImmutableParameter parameter)
			// There shouldn't be any KNFExceptions because the type readers/preconditions
			// are already setting default values and checking for undefined values
			=> structured[parameter.OriginalParameterName];

		private sealed class GeneratedNamedArgumentsParameterPrecondition
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

		private sealed class GeneratedNamedArgumentsTypeReader : NamedArgumentsTypeReaderBase<T>
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
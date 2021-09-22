using System.Collections.Immutable;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	/// <inheritdoc />
	/// <typeparam name="T"></typeparam>
	public class NamedArgumentsCommand<T> : GeneratedCommand
		where T : IDictionary<string, object?>, new()
	{
		/// <inheritdoc />
		public override int MaxLength => int.MaxValue;
		/// <inheritdoc />
		public override int MinLength => 0;
		/// <inheritdoc />
		public override IReadOnlyList<IImmutableParameter> Parameters { get; }

		/// <summary>
		/// Creates a new <see cref="NamedArgumentsCommand{T}"/>.
		/// </summary>
		/// <inheritdoc cref="GeneratedCommand(IImmutableCommand, int)"/>
		public NamedArgumentsCommand(IImmutableCommand source, int priorityDifference)
			: base(source, priorityDifference)
		{
			var parameters = ImmutableArray.CreateBuilder<IImmutableParameter>(1);
			try
			{
				var parameter = Commands.Linq.Parameters.Create<T>("NamedArgDictionary");
				parameter.AddParameterPrecondition(new GeneratedNamedArgumentsParameterPrecondition(Source))
					.SetTypeReader(new GeneratedNamedArgumentsTypeReader(Source))
					.AddAttribute(new RemainderAttribute());
				parameters.Add(parameter.ToImmutable());
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Unable to build {typeof(T).Name} " +
					$"parameter for '{source.Paths?.FirstOrDefault()}'.", e);
			}
			Parameters = parameters.MoveToImmutable();
		}

		/// <inheritdoc />
		public override ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
		{
			T dict;
			try
			{
				dict = (T)args.Single()!;
			}
			catch (Exception e)
			{
				throw new ArgumentException($"Expected one {typeof(T)} and no " +
					$"other arguments for '{Source.Paths?.FirstOrDefault()}'.", e);
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
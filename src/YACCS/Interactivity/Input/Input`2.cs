using System.Collections.Immutable;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Interactivity.Input;

/// <summary>
/// The base class for handling input.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TInput"></typeparam>
public abstract class Input<TContext, TInput>
	: Interactivity<TContext, TInput>, IInput<TContext, TInput>
	where TContext : IContext
{
	/// <summary>
	/// A singleton instance of an empty command.
	/// </summary>
	protected static IImmutableCommand EmptyCommand { get; } = new DelegateCommand(
		static () => { },
		new[] { LocalizedPath.New("Input") },
		typeof(TContext)
	).ToImmutable();

	/// <summary>
	/// The type readers used for parsing values.
	/// </summary>
	protected IReadOnlyDictionary<Type, ITypeReader> TypeReaders { get; }

	/// <summary>
	/// Creates a new <see cref="Input{TContext, TInput}"/>.
	/// </summary>
	/// <param name="typeReaders">
	/// <inheritdoc cref="TypeReaders" path="/summary"/>
	/// </param>
	protected Input(IReadOnlyDictionary<Type, ITypeReader> typeReaders)
	{
		TypeReaders = typeReaders;
	}

	/// <inheritdoc />
	public virtual Task<ITypeReaderResult<TValue>> GetAsync<TValue>(
		TContext context,
		InputOptions<TContext, TInput, TValue> options)
	{
		var eventTrigger = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);
		return HandleInteractionAsync(context, options, eventTrigger, async input =>
		{
			foreach (var criterion in options.Criteria ?? [])
			{
				var result = await criterion.JudgeAsync(context, input).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}

			var tr = options.TypeReader ?? TypeReaders.GetTypeReader<TValue>();
			var trResult = await tr.ReadAsync(context, new[] { GetInputString(input) }).ConfigureAwait(false);
			if (!trResult.InnerResult.IsSuccess)
			{
				return trResult.InnerResult;
			}

			var meta = new CommandMeta(EmptyCommand, EmptyParameter<TValue>.Instance);
			foreach (var precondition in options.Preconditions ?? [])
			{
				var result = await precondition.CheckAsync(
					meta,
					context,
					trResult.Value
				).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}

			eventTrigger.SetResult(trResult.Value!);
			return Success.Instance;
		});
	}

	/// <summary>
	/// Converts <paramref name="input"/> into a <see cref="string"/> so it can be parsed.
	/// </summary>
	/// <param name="input">The input to convert.</param>
	/// <returns>A string representing <paramref name="input"/>.</returns>
	protected abstract string GetInputString(TInput input);

	/// <inheritdoc cref="IImmutableParameter"/>
	protected sealed class EmptyParameter<T> : IImmutableParameter
	{
		private static readonly object[] _Attributes = [];
		private static readonly string _Name = $"Input_{typeof(T).FullName}";

		/// <summary>
		/// A singleton instance of <see cref="EmptyParameter{T}"/>.
		/// </summary>
		public static EmptyParameter<T> Instance { get; } = new();

		IReadOnlyList<object> IImmutableEntity.Attributes => _Attributes;
		IEnumerable<object> IQueryableEntity.Attributes => _Attributes;
		object? IImmutableParameter.DefaultValue => null;
		bool IImmutableParameter.HasDefaultValue => false;
		int? IImmutableParameter.Length => int.MaxValue;
		string IQueryableParameter.OriginalParameterName => _Name;
		string IImmutableParameter.ParameterName => _Name;
		Type IQueryableParameter.ParameterType { get; } = typeof(T);
		IReadOnlyDictionary<string, IReadOnlyList<IParameterPrecondition>> IImmutableParameter.Preconditions { get; }
			= ImmutableDictionary<string, IReadOnlyList<IParameterPrecondition>>.Empty;
		string IImmutableEntity.PrimaryId { get; } = typeof(T).FullName;
		ITypeReader? IImmutableParameter.TypeReader => null;
	}
}
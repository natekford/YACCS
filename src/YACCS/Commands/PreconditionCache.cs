using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public class PreconditionCache
	{
		private readonly IContext _Context;
		private readonly Dictionary<PPKey, IResult> _ParameterPreconditions = new();
		private readonly Dictionary<PKey, IResult> _Preconditions = new();
		private readonly Dictionary<TRKey, ITypeReaderResult> _TypeReaders = new();

		public PreconditionCache(IContext context)
		{
			_Context = context;
		}

		public ValueTask<IResult> GetResultAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IParameterPrecondition precondition,
			object? value)
		{
			var key = new PPKey(precondition, value);
			if (_ParameterPreconditions.TryGetValue(key, out var result))
			{
				return new ValueTask<IResult>(result);
			}
			return new ValueTask<IResult>(
				GetUncachedResultAsync(command, parameter, precondition, value, key));
		}

		public ValueTask<IResult> GetResultAsync(
			IImmutableCommand command,
			IPrecondition precondition)
		{
			var key = new PKey(precondition, command);
			if (_Preconditions.TryGetValue(key, out var result))
			{
				return new ValueTask<IResult>(result);
			}
			return new ValueTask<IResult>(
				GetUncachedResultAsync(command, precondition, key));
		}

		public ValueTask<ITypeReaderResult> GetResultAsync(
			ITypeReader reader,
			ReadOnlyMemory<string> value)
		{
			var key = new TRKey(reader, value);
			if (_TypeReaders.TryGetValue(key, out var result))
			{
				return new ValueTask<ITypeReaderResult>(result);
			}
			return new ValueTask<ITypeReaderResult>(
				GetUncachedResultAsync(reader, value, key));
		}

		private async Task<IResult> GetUncachedResultAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IParameterPrecondition precondition,
			object? value,
			PPKey key)
		{
			var result = await precondition.CheckAsync(
				command,
				parameter,
				_Context,
				value
			).ConfigureAwait(false);
			return _ParameterPreconditions[key] = result;
		}

		private async Task<IResult> GetUncachedResultAsync(
			IImmutableCommand command,
			IPrecondition precondition,
			PKey key)
		{
			var result = await precondition.CheckAsync(command, _Context).ConfigureAwait(false);
			return _Preconditions[key] = result;
		}

		private async Task<ITypeReaderResult> GetUncachedResultAsync(
			ITypeReader reader,
			ReadOnlyMemory<string> input,
			TRKey key)
		{
			var result = await reader.ReadAsync(_Context, input).ConfigureAwait(false);
			return _TypeReaders[key] = result;
		}

		private readonly struct PKey : IEquatable<PKey>
		{
			public IImmutableCommand Command { get; }
			public IPrecondition Precondition { get; }

			public PKey(IPrecondition precondition, IImmutableCommand command)
			{
				Precondition = precondition;
				Command = command;
			}

			public bool Equals(PKey other)
				=> Command == other.Command && Precondition == other.Precondition;
		}

		private readonly struct PPKey : IEquatable<PPKey>
		{
			public IParameterPrecondition Precondition { get; }
			public object? Value { get; }

			public PPKey(IParameterPrecondition precondition, object? value)
			{
				Precondition = precondition;
				Value = value;
			}

			public bool Equals(PPKey other)
				=> Precondition == other.Precondition && Value == other.Value;
		}

		private readonly struct TRKey : IEquatable<TRKey>
		{
			public ITypeReader TypeReader { get; }
			public ReadOnlyMemory<string> Value { get; }

			public TRKey(ITypeReader typeReader, ReadOnlyMemory<string> value)
			{
				TypeReader = typeReader;
				Value = value;
			}

			public bool Equals(TRKey other)
				=> TypeReader == other.TypeReader && Value.Equals(other.Value);
		}
	}
}
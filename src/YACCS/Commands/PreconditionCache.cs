using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public class PreconditionCache
	{
		private readonly IContext _Context;
		private readonly Dictionary<object, IResult> _ParameterPreconditions
			= new Dictionary<object, IResult>();
		private readonly Dictionary<object, IResult> _Preconditions
			= new Dictionary<object, IResult>();
		private readonly Dictionary<object, ITypeReaderResult> _TypeReaders
			= new Dictionary<object, ITypeReaderResult>();

		public PreconditionCache(IContext context)
		{
			_Context = context;
		}

		public async Task<IResult> GetResultAsync(
			ParameterInfo parameter,
			IParameterPrecondition precondition,
			object? value)
		{
			var key = (value, precondition);
			if (!_ParameterPreconditions.TryGetValue(key, out var result))
			{
				result = await precondition.CheckAsync(
					parameter,
					_Context,
					value
				).ConfigureAwait(false);
				_ParameterPreconditions[key] = result;
			}
			return result;
		}

		public async Task<IResult> GetResultAsync(IImmutableCommand command, IPrecondition precondition)
		{
			var key = precondition;
			if (!_Preconditions.TryGetValue(key, out var result))
			{
				result = await precondition.CheckAsync(command, _Context).ConfigureAwait(false);
				_Preconditions[key] = result;
			}
			return result;
		}

		public async Task<ITypeReaderResult> GetResultAsync(ITypeReader reader, string value)
		{
			var key = (value, reader);
			if (!_TypeReaders.TryGetValue(key, out var result))
			{
				result = await reader.ReadAsync(_Context, value).ConfigureAwait(false);
				_TypeReaders[key] = result;
			}
			return result;
		}
	}
}
using System;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.StructuredArguments
{
	public abstract class StructuredArgumentsCommand<T> : GeneratedCommand
	{
		protected StructuredArgumentsCommand(IImmutableCommand source) : base(source)
		{
		}

		public override ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
		{
			T structured;
			try
			{
				structured = (T)args.Single()!;
			}
			catch (Exception e)
			{
				throw new ArgumentException($"Expected one {typeof(T)} and no " +
					$"other arguments for '{Source.Names?.FirstOrDefault()}'.", e);
			}

			var values = new object?[Source.Parameters.Count];
			for (var i = 0; i < values.Length; ++i)
			{
				values[i] = GetValue(structured, Source.Parameters[i]);
			}
			return Source.ExecuteAsync(context, values);
		}

		protected abstract object? GetValue(T structured, IImmutableParameter parameter);
	}
}
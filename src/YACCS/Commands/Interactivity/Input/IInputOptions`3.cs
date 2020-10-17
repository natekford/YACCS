using System.Collections.Generic;

using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity.Input
{
	public interface IInputOptions<in TContext, in TInput, TValue>
		: IInteractivityOptions<TContext, TInput>
		where TContext : IContext
	{
		IEnumerable<IParameterPrecondition<TContext, TValue>> Preconditions { get; }
		ITypeReader<TValue>? TypeReader { get; }
	}
}
using System;
using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity
{
	public abstract class InteractiveBase<TContext, TInput> where TContext : IContext
	{
		protected virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

		protected abstract void Subscribe(TContext context, OnInput onInput, Guid id);

		protected abstract void Unsubscribe(TContext context, OnInput onInput, Guid id);

		protected delegate Task OnInput(TInput input);
	}
}
namespace YACCS.Commands.Interactivity
{
	public abstract class InteractiveCommandGroup<TContext, TInput> : CommandGroup<TContext>
		where TContext : IContext
	{
		public IInputGetter<TContext, TInput> ValueGetter { get; set; } = null!;
	}
}
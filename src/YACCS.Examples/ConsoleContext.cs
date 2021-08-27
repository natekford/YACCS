using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;

namespace YACCS.Examples
{
	public sealed class ConsoleContext : IContext<string>, IDisposable
	{
		public Guid Id { get; } = Guid.NewGuid();
		public string Input { get; }
		public IServiceScope Scope { get; }
		public IServiceProvider Services => Scope.ServiceProvider;
		public DateTime Start { get; } = DateTime.UtcNow;
		string IContext<string>.Source => Input;
		object IContext.Source => Input;

		public ConsoleContext(IServiceScope scope, string input)
		{
			Scope = scope;
			Input = input;
		}

		public void Dispose()
		{
			Scope.ServiceProvider.GetRequiredService<ConsoleHandler>().ReleaseIOLocks();
			Scope.Dispose();
		}
	}
}
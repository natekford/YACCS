using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;

using YACCS.Commands;

namespace YACCS.Examples;

public sealed class ConsoleContext : IContext, IDisposable, IMessagable
{
	private readonly ConsoleHandler _Console;

	public Guid Id { get; } = Guid.NewGuid();
	public string Input { get; }
	public IServiceScope Scope { get; }
	public IServiceProvider Services => Scope.ServiceProvider;
	public DateTime Start { get; } = DateTime.UtcNow;
	public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
	object IContext.Source => Input;

	public ConsoleContext(IServiceScope scope, string input)
	{
		Scope = scope;
		Input = input;
		_Console = Scope.ServiceProvider.GetRequiredService<ConsoleHandler>();
	}

	public void Dispose()
	{
		_Console.ReleaseIOLock();
		Stopwatch.Stop();
		Scope.Dispose();
	}

	public Task SendMessageAsync(string message)
	{
		_Console.WriteLine(message);
		_Console.WriteLine();
		return Task.CompletedTask;
	}
}
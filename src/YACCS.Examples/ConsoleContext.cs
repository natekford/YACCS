using System;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands;

namespace YACCS.Examples
{
	public class ConsoleContext : IContext<string>, IDisposable
	{
		public Guid Id { get; } = Guid.NewGuid();
		public string Input { get; }
		public IServiceScope Scope { get; }
		public IServiceProvider Services => Scope.ServiceProvider;
		string IContext<string>.Source => Input;
		object IContext.Source => Input;

		public ConsoleContext(IServiceScope scope, string input)
		{
			Scope = scope;
			Input = input;
		}

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

		public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
			=> Scope.Dispose();
	}
}
namespace YACCS.Tests;

public sealed class FakeServiceProvider : IServiceProvider
{
	public static FakeServiceProvider Instance { get; } = new();

	public object? GetService(Type serviceType) => null;
}
using System;

namespace YACCS;

/// <summary>
/// An empty <see cref="IServiceProvider"/>.
/// </summary>
public sealed class EmptyServiceProvider : IServiceProvider
{
	/// <summary>
	/// A singletone instance of <see cref="EmptyServiceProvider"/>.
	/// </summary>
	public static EmptyServiceProvider Instance { get; } = new();

	private EmptyServiceProvider()
	{
	}

	/// <inheritdoc />
	public object? GetService(Type serviceType) => null;
}
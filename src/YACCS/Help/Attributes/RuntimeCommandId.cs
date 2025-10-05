using System;
using System.Threading;

namespace YACCS.Help.Attributes;

/// <inheritdoc cref="IRuntimeCommandId" />
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RuntimeCommandId : Attribute, IRuntimeCommandId
{
	private static int _Count;

	/// <summary>
	/// The current runtime id to use.
	/// </summary>
	public static int Count
	{
		get => _Count;
		set => _Count = value;
	}

	/// <inheritdoc />
	public int Id { get; } = Interlocked.Increment(ref _Count);
}
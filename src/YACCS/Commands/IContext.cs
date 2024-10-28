using System;

namespace YACCS.Commands;

/// <summary>
/// Defines a context used to invoke commands.
/// </summary>
public interface IContext
{
	/// <summary>
	/// The unique id for this context.
	/// </summary>
	Guid Id { get; }
	/// <summary>
	/// The services to use for dependency injection.
	/// </summary>
	IServiceProvider Services { get; }
	/// <summary>
	/// The item which initialized commands being searched for.
	/// </summary>
	object Source { get; }
}
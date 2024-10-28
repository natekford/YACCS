using System.Collections.Generic;

namespace YACCS.Commands.Attributes;

/// <summary>
/// An attribute for declaring a method as a command.
/// </summary>
public interface ICommandAttribute
{
	/// <summary>
	/// If <see langword="true"/>, descendant classes will instantiate this command
	/// along with all commands defined in their class.
	/// </summary>
	bool AllowInheritance { get; }
	/// <summary>
	/// The possible values to use when invoking this command.
	/// </summary>
	IReadOnlyList<string> Names { get; }
}
using System.Collections.Generic;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments;

/// <summary>
/// Provides a way to see the parameters of a named argument type.
/// </summary>
public interface INamedArgumentParameters
{
	/// <summary>
	/// The parameters this named argument type supports.
	/// </summary>
	/// <remarks>
	/// The keys are the current localized parameter name, NOT the original parameter name.
	/// </remarks>
	public IReadOnlyDictionary<string, IImmutableParameter> Parameters { get; }
}
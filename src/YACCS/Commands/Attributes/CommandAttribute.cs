using System.Collections.Immutable;

namespace YACCS.Commands.Attributes;

/// <inheritdoc cref="ICommandAttribute"/>
/// <inheritdoc cref="CommandAttribute()"/>
/// <param name="names">
/// <inheritdoc cref="Names" path="/summary"/>
/// </param>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public class CommandAttribute(IReadOnlyList<string> names)
	: Attribute, ICommandAttribute
{
	/// <inheritdoc />
	public bool AllowInheritance { get; set; }
	/// <inheritdoc />
	public virtual IReadOnlyList<string> Names { get; } = names;

	/// <inheritdoc cref="CommandAttribute()"/>
	/// <param name="names">
	/// <inheritdoc cref="Names" path="/summary"/>
	/// </param>
	public CommandAttribute(params string[] names) : this(names.ToImmutableArray())
	{
	}

	/// <summary>
	/// Creates a new <see cref="CommandAttribute"/>.
	/// </summary>
	public CommandAttribute() : this(ImmutableArray<string>.Empty)
	{
	}
}
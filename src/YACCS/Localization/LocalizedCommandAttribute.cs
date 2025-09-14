using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using YACCS.Commands.Attributes;

namespace YACCS.Localization;

/// <inheritdoc />
/// <summary>
/// Creates a new <see cref="LocalizedCommandAttribute"/>.
/// </summary>
/// <param name="keys">
/// <inheritdoc cref="Keys" path="/summary"/>
/// </param>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public class LocalizedCommandAttribute(IReadOnlyList<string> keys)
	: CommandAttribute(keys)
{
	/// <summary>
	/// The keys for localization.
	/// </summary>
	public IReadOnlyList<string> Keys { get; } = keys;
	/// <inheritdoc />
	public override IReadOnlyList<string> Names => Localized.GetCurrent();
	/// <summary>
	/// The localized names.
	/// </summary>
	protected virtual Localized<IReadOnlyList<string>> Localized { get; }
		= new(_ => [.. keys.Select(x => Localize.This(x))]);

	/// <inheritdoc cref="LocalizedCommandAttribute(IReadOnlyList{string})"/>
	public LocalizedCommandAttribute(params string[] keys) : this(keys.ToImmutableArray())
	{
	}

	/// <inheritdoc cref="LocalizedCommandAttribute(IReadOnlyList{string})"/>
	public LocalizedCommandAttribute() : this(ImmutableArray<string>.Empty)
	{
	}
}
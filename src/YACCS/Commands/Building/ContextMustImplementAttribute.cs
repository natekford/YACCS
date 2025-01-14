﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Attributes;

namespace YACCS.Commands.Building;

/// <summary>
/// Specifies what types a context must implement.
/// </summary>
/// <param name="types">
/// <inheritdoc cref="Types" path="/summary"/>
/// </param>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
public sealed class ContextMustImplementAttribute(IReadOnlyList<Type> types)
	: Attribute, IContextConstraint
{
	/// <summary>
	/// The types the context must implement.
	/// </summary>
	public IReadOnlyList<Type> Types { get; } = types;

	/// <inheritdoc cref="ContextMustImplementAttribute(IReadOnlyList{Type})"/>
	public ContextMustImplementAttribute(params Type[] types)
		: this(types.ToImmutableArray())
	{
	}

	/// <inheritdoc />
	public bool DoesTypeSatisfy(Type context)
	{
		foreach (var constraint in Types)
		{
			if (!constraint.IsAssignableFrom(context))
			{
				return false;
			}
		}
		return true;
	}
}
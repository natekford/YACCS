﻿using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using YACCS.Preconditions;

namespace YACCS.Commands.Attributes;

/// <summary>
/// Utilities for attributes.
/// </summary>
public static class AttributeUtils
{
	/// <summary>
	/// Specifies that this attribute can only target <see cref="AttributeTargets.Class"/>
	/// and <see cref="AttributeTargets.Method"/>.
	/// </summary>
	public const AttributeTargets COMMANDS = 0
		| AttributeTargets.Class
		| AttributeTargets.Method;

	/// <summary>
	/// Specifies that this attribute can only target <see cref="AttributeTargets.Parameter"/>,
	/// <see cref="AttributeTargets.Property"/>, and <see cref="AttributeTargets.Field"/>.
	/// </summary>
	public const AttributeTargets PARAMETERS = 0
		| AttributeTargets.Parameter
		| AttributeTargets.Property
		| AttributeTargets.Field;

	internal static void AddPrecondition<TPrecondition>(
		this ConcurrentDictionary<string, List<TPrecondition>> dict,
		TPrecondition precondition)
		where TPrecondition : IGroupablePrecondition
	{
		foreach (var group in precondition.Groups.DefaultIfEmpty(string.Empty))
		{
			dict.GetOrAdd(group, _ => []).Add(precondition);
		}
	}

	internal static TValue ThrowIfDuplicate<TAttribute, TValue>(
		this TAttribute attribute,
		Func<TAttribute, TValue> converter,
		ref int count)
	{
		if (count > 0)
		{
			throw new InvalidOperationException($"Duplicate {typeof(TAttribute).Name} attribute.");
		}

		++count;
		return converter.Invoke(attribute);
	}

	internal static FrozenDictionary<string, IReadOnlyList<TPrecondition>> ToImmutablePreconditions<TPrecondition>(
		this IDictionary<string, List<TPrecondition>> dict)
	{
		return dict.ToFrozenDictionary(
			x => x.Key,
			x => (IReadOnlyList<TPrecondition>)x.Value.ToImmutableArray()
		);
	}
}
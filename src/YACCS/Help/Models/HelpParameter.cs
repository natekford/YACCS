﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.NamedArguments;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models;

/// <summary>
/// Information to display about a parameter in a help command.
/// </summary>
/// <param name="item">The parameter to present information about.</param>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class HelpParameter(IImmutableParameter item)
	: HelpItem<IImmutableParameter>(item, item.Attributes.Select(x => x.Value), x => x is not IParameterPrecondition)
{
	/// <summary>
	/// Whether or not this parameter is a remainder.
	/// </summary>
	public virtual bool IsRemainder { get; } = item.Length is null;
	/// <summary>
	/// Sub parameters for named arguments.
	/// </summary>
	public virtual IReadOnlyList<HelpParameter>? NamedArguments { get; }
		= item.Preconditions
		.SelectMany(x => x.Value)
		.OfType<INamedArgumentParameters>()
		.SingleOrDefault()?
		.Parameters?
		.Select(x => new HelpParameter(x))?
		.ToImmutableArray();
	/// <summary>
	/// The type of this parameter.
	/// </summary>
	public virtual HelpItem<Type> ParameterType { get; }
		= item.ParameterType.ToHelpItem();
	/// <summary>
	/// The precondition groups of this parameter.
	/// </summary>
	public virtual IReadOnlyDictionary<string, ILookup<Op, HelpItem<IParameterPrecondition>>> Preconditions { get; }
		= item.Preconditions.ToHelpItems();
	/// <summary>
	/// The specified type reader of this parameter.
	/// </summary>
	public virtual HelpItem<ITypeReader>? TypeReader { get; }
		= item.TypeReader is ITypeReader tr ? new(tr) : null;
	private string DebuggerDisplay => Item.FormatForDebuggerDisplay();
}
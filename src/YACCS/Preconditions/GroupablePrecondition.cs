using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Help.Attributes;

namespace YACCS.Preconditions;

/// <summary>
/// The base class for a groupable precondition attribute.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public abstract class GroupablePrecondition
	: Attribute, IGroupablePrecondition, ISummarizableAttribute
{
	/// <inheritdoc />
	public virtual string[] Groups { get; set; } = [];
	/// <inheritdoc />
	public virtual Op Op { get; set; } = Op.And;
	IReadOnlyList<string> IGroupablePrecondition.Groups => Groups;

	/// <inheritdoc />
	public abstract ValueTask<string> GetSummaryAsync(IContext context, IFormatProvider? formatProvider = null);
}
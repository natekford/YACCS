using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace YACCS.Commands.Models;

/// <summary>
/// The base class for most command objects.
/// </summary>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public abstract class Entity : IMutableEntity
{
	/// <inheritdoc />
	public IList<object> Attributes { get; set; }
	IEnumerable<object> IQueryableEntity.Attributes => Attributes;
	/// <summary>
	/// The attributes retrieved in the constructor.
	/// </summary>
	protected ImmutableArray<object> BaseAttributes { get; }
	private string DebuggerDisplay => $"Attribute Count = {Attributes.Count}";

	/// <summary>
	/// Creates a new <see cref="Entity"/> and sets <see cref="Attributes"/>
	/// and <see cref="BaseAttributes"/> with the attributes from
	/// <paramref name="provider"/>.
	/// </summary>
	/// <param name="provider"></param>
	protected Entity(ICustomAttributeProvider? provider)
	{
		BaseAttributes = provider?.GetCustomAttributes(true)?.ToImmutableArray() ?? [];
		Attributes = BaseAttributes.ToList();
	}
}
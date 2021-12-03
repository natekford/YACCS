using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace YACCS.Commands.Models;

/// <summary>
/// The base class for most command objects.
/// </summary>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public abstract class EntityBase : IEntityBase
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
	/// Creates a new <see cref="EntityBase"/> and sets <see cref="Attributes"/>
	/// and <see cref="BaseAttributes"/> with the attributes from
	/// <paramref name="provider"/>.
	/// </summary>
	/// <param name="provider"></param>
	protected EntityBase(ICustomAttributeProvider? provider)
	{
		var attributes = Array.Empty<object>();
		if (provider is not null)
		{
			attributes = provider.GetCustomAttributes(true);
		}

		Attributes = new List<object>(attributes);
		BaseAttributes = Unsafe.As<object[], ImmutableArray<object>>(ref attributes);
	}
}

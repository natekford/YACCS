using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
	public IList<AttributeInfo> Attributes { get; set; }
	IEnumerable<AttributeInfo> IQueryableEntity.Attributes => Attributes;
	private string DebuggerDisplay => $"Attribute Count = {Attributes.Count}";

	/// <summary>
	/// Creates a new <see cref="Entity"/> and sets <see cref="Attributes"/>
	/// with attributes from <paramref name="provider"/>.
	/// </summary>
	/// <param name="provider"></param>
	protected Entity(ICustomAttributeProvider? provider)
	{
		Attributes = [];
		if (provider is null)
		{
			return;
		}

		var (direct, inherited) = GetAttributes(provider);
		foreach (var attribute in direct)
		{
			Attributes.Add(new(provider, AttributeInfo.ON_METHOD, attribute));
		}
		foreach (var attribute in inherited)
		{
			Attributes.Add(new(provider, AttributeInfo.ON_METHOD_INHERITED, attribute));
		}
	}

	/// <summary>
	/// Similar to <see cref="ICustomAttributeProvider.GetCustomAttributes(bool)"/>
	/// but returns both the direct and inherited as separate enumerables.
	/// </summary>
	/// <param name="provider"></param>
	/// <returns></returns>
	protected static (IEnumerable<object> Direct, IEnumerable<object> Inherited) GetAttributes(
		ICustomAttributeProvider provider)
	{
		static bool IsValid(object[] direct, object[] inherited)
		{
			if (inherited.Length < direct.Length)
			{
				Debug.Fail("GetCustomAttributes Inherited is shorter than Direct");
			}

			var dict = new ConcurrentDictionary<Type, int>(1, direct.Length);
			foreach (var item in direct.Select(x => x.GetType()))
			{
				dict.AddOrUpdate(item, 1, (_, v) => ++v);
			}
			foreach (var item in inherited.Take(direct.Length).Select(x => x.GetType()))
			{
				--dict[item];
			}

			foreach (var (key, value) in dict)
			{
				if (value != 0)
				{
					Debug.Fail("GetCustomAttributes Inherited and Direct did not match.");
				}
			}
			return true;
		}

		// Getting direct attributes is easy, just set inherit to false
		var direct = provider.GetCustomAttributes(inherit: false);
		// Getting inherited is a bit trickier, this relies on GetCustomAttributes
		// implementation details
		// Order is NOT guaranteed within the groups of attributes returned, but
		// I believe the order of the groups themselves is the same each time
		// i.e. direct attributes, parent attributes, parent parent attributes, etc
		var inherited = provider.GetCustomAttributes(inherit: true);
		Debug.Assert(IsValid(direct, inherited));
		return (direct, inherited.Skip(direct.Length));
	}
}
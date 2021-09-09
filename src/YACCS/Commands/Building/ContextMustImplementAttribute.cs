using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Attributes;

namespace YACCS.Commands.Building
{
	/// <summary>
	/// Specifies what types a context must implement.
	/// </summary>
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
	public sealed class ContextMustImplementAttribute : Attribute, IContextConstraint
	{
		/// <summary>
		/// The types the context must implement.
		/// </summary>
		public IReadOnlyList<Type> Types { get; }

		/// <inheritdoc cref="ContextMustImplementAttribute(IReadOnlyList{Type})"/>
		public ContextMustImplementAttribute(params Type[] types)
			: this(types.ToImmutableArray())
		{
		}

		/// <summary>
		/// Creates a new <see cref="ContextMustImplementAttribute"/>.
		/// </summary>
		/// <param name="types">
		/// <inheritdoc cref="Types" path="/summary"/>
		/// </param>
		public ContextMustImplementAttribute(IReadOnlyList<Type> types)
		{
			Types = types;
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
}
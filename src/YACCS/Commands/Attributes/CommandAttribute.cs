using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands.Attributes
{
	/// <inheritdoc cref="ICommandAttribute"/>
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class CommandAttribute : Attribute, ICommandAttribute
	{
		/// <inheritdoc />
		public bool AllowInheritance { get; set; }
		/// <inheritdoc />
		public virtual IReadOnlyList<string> Names { get; }

		/// <summary>
		/// Creates a new <see cref="CommandAttribute"/> and sets <see cref="Names"/>
		/// to <paramref name="names"/>.
		/// </summary>
		/// <param name="names"></param>
		public CommandAttribute(params string[] names) : this(names.ToImmutableArray())
		{
		}

		/// <summary>
		/// Creates a new <see cref="CommandAttribute"/> and sets <see cref="Names"/>
		/// to <paramref name="names"/>.
		/// </summary>
		/// <param name="names"></param>
		public CommandAttribute(IReadOnlyList<string> names)
		{
			Names = names;
		}

		/// <summary>
		/// Creates a new <see cref="CommandAttribute"/>.
		/// </summary>
		public CommandAttribute() : this(ImmutableArray<string>.Empty)
		{
		}
	}
}
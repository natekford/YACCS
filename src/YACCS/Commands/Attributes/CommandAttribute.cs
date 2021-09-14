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

		/// <inheritdoc cref="CommandAttribute()"/>
		/// <param name="names">
		/// <inheritdoc cref="Names" path="/summary"/>
		/// </param>
		public CommandAttribute(params string[] names) : this(names.ToImmutableArray())
		{
		}

		/// <inheritdoc cref="CommandAttribute()"/>
		/// <param name="names">
		/// <inheritdoc cref="Names" path="/summary"/>
		/// </param>
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
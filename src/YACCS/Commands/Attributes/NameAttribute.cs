using System;

namespace YACCS.Commands.Attributes
{
	/// <inheritdoc cref="INameAttribute"/>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class NameAttribute : Attribute, INameAttribute
	{
		/// <inheritdoc />
		public virtual string Name { get; }

		/// <summary>
		/// Creates a new <see cref="NameAttribute"/> and sets <see cref="Name"/>
		/// to <paramref name="name"/>.
		/// </summary>
		/// <param name="name"></param>
		public NameAttribute(string name)
		{
			Name = name;
		}
	}
}
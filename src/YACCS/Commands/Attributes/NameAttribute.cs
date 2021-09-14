namespace YACCS.Commands.Attributes
{
	/// <inheritdoc cref="INameAttribute"/>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class NameAttribute : Attribute, INameAttribute
	{
		/// <inheritdoc />
		public virtual string Name { get; }

		/// <summary>
		/// Creates a new <see cref="NameAttribute"/>.
		/// </summary>
		/// <param name="name">
		/// <inheritdoc cref="Name" path="/summary"/>
		/// </param>
		public NameAttribute(string name)
		{
			Name = name;
		}
	}
}
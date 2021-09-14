namespace YACCS.TypeReaders
{
	/// <summary>
	/// Defines a method for generating a type reader from a <see cref="Type"/>.
	/// </summary>
	public interface ITypeReaderGeneratorAttribute
	{
		/// <summary>
		/// Creates a new <see cref="ITypeReader"/>.
		/// </summary>
		/// <param name="type">The type to use for creation.</param>
		/// <returns>A type reader.</returns>
		ITypeReader GenerateTypeReader(Type type);
	}
}
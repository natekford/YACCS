using System;

using YACCS.TypeReaders;

namespace YACCS.Commands.Attributes
{
	/// <inheritdoc cref="IOverrideTypeReaderAttribute"/>
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false)]
	public class OverrideTypeReaderAttribute : Attribute, IOverrideTypeReaderAttribute
	{
		/// <inheritdoc />
		public ITypeReader Reader { get; }

		/// <summary>
		/// Creates a new <see cref="OverrideTypeReaderAttribute"/> and sets
		/// <see cref="Reader"/> to an instance of <paramref name="type"/>.
		/// </summary>
		/// <param name="type"></param>
		public OverrideTypeReaderAttribute(Type type)
		{
			Reader = type.CreateInstance<ITypeReader>();
		}
	}
}
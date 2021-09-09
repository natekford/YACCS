using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Commands.Attributes
{
	/// <summary>
	/// An attribute used for setting <see cref="IImmutableParameter.TypeReader"/>.
	/// </summary>
	public interface IOverrideTypeReaderAttribute
	{
		/// <summary>
		/// The reader to use.
		/// </summary>
		ITypeReader Reader { get; }
	}
}
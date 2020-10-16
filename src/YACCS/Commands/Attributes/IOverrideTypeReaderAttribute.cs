using YACCS.TypeReaders;

namespace YACCS.Commands.Attributes
{
	public interface IOverrideTypeReaderAttribute
	{
		ITypeReader Reader { get; }
	}
}
using System;

namespace YACCS.TypeReaders
{
	public interface INullValidator
	{
		bool IsNull(ReadOnlyMemory<string?> input);
	}
}
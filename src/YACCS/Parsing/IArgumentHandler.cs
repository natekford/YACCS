using System;

namespace YACCS.Parsing
{
	public interface IArgumentHandler
	{
		string Join(ReadOnlyMemory<string> args);

		bool TrySplit(ReadOnlySpan<char> input, out ReadOnlyMemory<string> args);
	}
}
using System.IO;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a <see cref="DirectoryInfo"/>.
/// </summary>
public class DirectoryInfoTypeReader() : TryParseTypeReader<DirectoryInfo>(TryParse)
{
	private static bool TryParse(string s, out DirectoryInfo result)
	{
		if (Directory.Exists(s))
		{
			result = new DirectoryInfo(s);
			return true;
		}
		else
		{
			result = null!;
			return false;
		}
	}
}
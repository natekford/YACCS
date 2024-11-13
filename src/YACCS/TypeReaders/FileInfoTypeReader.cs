using System.IO;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a <see cref="FileInfo"/>.
/// </summary>
public class FileInfoTypeReader() : TryParseTypeReader<FileInfo>(TryParse)
{
	private static bool TryParse(string s, out FileInfo result)
	{
		if (File.Exists(s))
		{
			result = new FileInfo(s);
			return true;
		}
		else
		{
			result = null!;
			return false;
		}
	}
}
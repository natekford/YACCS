namespace YACCS.TypeReaders;

/// <summary>
/// Parses a <see cref="FileInfo"/>.
/// </summary>
public class FileInfoTypeReader : TryParseTypeReader<FileInfo>
{
	/// <summary>
	/// Creates a new <see cref="FileInfoTypeReader"/>.
	/// </summary>
	public FileInfoTypeReader() : base(TryParse)
	{
	}

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
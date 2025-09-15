using System;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a <see cref="Uri"/>.
/// </summary>
public class UriTypeReader() : TryParseTypeReader<Uri>(TryParse)
{
	private static bool TryParse(string s, out Uri result)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			result = null!;
			return false;
		}

		try
		{
			// This is specifically for Discord/maybe markdown in general
			// Maybe remove?
			if (s.StartsWith('<') && s.EndsWith('>'))
			{
				s = s[1..^1];
			}

			return Uri.TryCreate(s, UriKind.Absolute, out result);
		}
		catch
		{
			result = null!;
			return false;
		}
	}
}
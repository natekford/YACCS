using System;

namespace YACCS.TypeReaders
{
	public class UriTypeReader : TryParseTypeReader<Uri>
	{
		public UriTypeReader() : base(TryParse)
		{
		}

		public static bool TryParse(string s, out Uri result)
		{
			if (string.IsNullOrWhiteSpace(s))
			{
				result = null!;
				return false;
			}

			try
			{
				if (s.StartsWith('<') && s.EndsWith('>'))
				{
					s = s[1..^1];
				}

				result = new Uri(s);
				return true;
			}
			catch
			{
				result = null!;
				return false;
			}
		}
	}
}
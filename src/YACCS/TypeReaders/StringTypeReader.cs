namespace YACCS.TypeReaders
{
	public class StringTypeReader : TryParseTypeReader<string?>
	{
		public StringTypeReader() : base(TryParse)
		{
		}

		private static bool TryParse(string s, out string result)
		{
			result = s;
			return true;
		}
	}
}